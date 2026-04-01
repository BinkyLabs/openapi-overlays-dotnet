using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

#pragma warning disable BOO002
public class OverlayReusableActionReferenceV1_1Tests
{
    [Fact]
    public void SerializeAsV1_1_ShouldWriteOnlySetActionOverridesAndReferenceFields()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Id = "errorResponse",
            ParametersValue = new Dictionary<string, JsonNode>
            {
                { "region", JsonValue.Create("us")! }
            },
            Description = "Override Description",
            Remove = false,
            Copy = "$.paths['/pets'].post.responses",
            Update = JsonNode.Parse("""
            {
                "summary": "Updated summary"
            }
            """)
        };

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "x-$ref": "#/components/actions/errorResponse",
    "x-parameterValues": {
        "region": "us"
    },
    "description": "Override Description",
    "update": {
        "summary": "Updated summary"
    },
    "copy": "$.paths['/pets'].post.responses"
}
""";

        // Act
        reference.SerializeAsV1_1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_1_ShouldIgnoreInterfaceFieldsWhenOnlyInheritedFromTargetAction()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Id = "errorResponse",
            TargetAction = new OverlayAction
            {
                Target = "$.info",
                Description = "from target action",
                Remove = true,
                Update = JsonNode.Parse("""{ "title": "x" }""")
            }
        };

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "x-$ref": "#/components/actions/errorResponse"
}
""";

        // Act
        reference.SerializeAsV1_1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "x-$ref": "#/components/actions/errorResponse",
            "x-parameterValues": {
                "region": "us",
                "stage": {
                    "name": "dev"
                }
            },
            "target": "$.paths['/pets'].get.responses",
            "description": "Override Description",
            "remove": false,
            "copy": "$.paths['/pets'].post.responses",
            "update": {
                "404": {
                    "description": "Not found"
                }
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var reference = OverlayV1_1Deserializer.LoadReusableActionReference(parseNode);

        // Assert
        Assert.Equal("errorResponse", reference.Id);
        Assert.Equal("#/components/actions/errorResponse", reference.Reference);
        Assert.NotNull(reference.ParametersValue);
        Assert.Equal("us", reference.ParametersValue["region"].GetValue<string>());
        Assert.Equal("dev", reference.ParametersValue["stage"]["name"]?.GetValue<string>());
        Assert.Equal("$.paths['/pets'].get.responses", reference.Target);
        Assert.Equal("Override Description", reference.Description);
        Assert.False(reference.Remove);
        Assert.Equal("$.paths['/pets'].post.responses", reference.Copy);
        Assert.Equal("Not found", reference.Update?["404"]?["description"]?.GetValue<string>());
    }
}
#pragma warning restore BOO002
