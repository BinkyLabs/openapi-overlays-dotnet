using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_2;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayReusableActionReferenceV1_2Tests
{
    [Fact]
    public void SerializeAsV1_2_ShouldWriteOnlySetActionOverridesAndReferenceFields()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                Target = "$.paths['/pets'].get.responses",
                Description = "Override Description",
            }
        };

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "$ref": "#/components/actions/errorResponse",
    "target": "$.paths['/pets'].get.responses",
    "description": "Override Description"
}
""";

        // Act
        reference.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_2_ShouldIgnoreInheritedFieldsFromTargetAction()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                Target = "$.some.target",
            },
            TargetAction = new OverlayReusableAction
            {
                Description = "Reusable description",
                Fields = new OverlayAction
                {
                    Description = "from target action",
                    Remove = true,
                    Update = JsonNode.Parse("""{ "title": "x" }""")
                }
            }
        };

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "$ref": "#/components/actions/errorResponse",
    "target": "$.some.target"
}
""";

        // Act
        reference.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_2_ShouldThrowOnMissingReferenceTarget()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse"
            }
        };

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Act + Assert
        var exception = Assert.Throws<InvalidOperationException>(() => reference.SerializeAsV1_2(writer));
        Assert.Contains("cannot be null or empty", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SerializeAsV1_2_WithMissingReference_ShouldThrow(string? missingReference)
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = missingReference
            }
        };

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Act + Assert
        var exception = Assert.Throws<InvalidOperationException>(() => reference.SerializeAsV1_2(writer));
        Assert.Contains("cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        var json = """
        {
            "$ref": "#/components/actions/errorResponse",
            "target": "$.paths['/pets'].get.responses",
            "description": "Override Description"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var reference = OverlayV1_2Deserializer.LoadReusableActionReference(jsonNode, parsingContext);

        Assert.Equal("errorResponse", reference.Reference.Id);
        Assert.Equal("#/components/actions/errorResponse", reference.Reference.Reference);
        Assert.Equal("$.paths['/pets'].get.responses", reference.Target);
        Assert.Equal("Override Description", reference.Description);
        Assert.Null(reference.Remove);
        Assert.Null(reference.Copy);
        Assert.Null(reference.Update);
    }
}