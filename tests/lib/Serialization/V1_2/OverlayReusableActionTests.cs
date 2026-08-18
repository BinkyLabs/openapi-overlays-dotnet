using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_2;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayReusableActionV1_2Tests
{
    [Fact]
    public void SerializeAsV1_2_ShouldWriteCorrectJson()
    {
        // Arrange
        var action = new OverlayReusableAction
        {
            Description = "Adds an error response",
            Fields = new OverlayAction
            {
                Description = "Test Description",
                Remove = true,
            }
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "description": "Adds an error response",
    "fields": {
        "description": "Test Description",
        "remove": true
    }
}
""";

        // Act
        action.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_2_WithNullFields_ShouldWriteEmptyFieldsObject()
    {
        // Arrange
        var action = new OverlayReusableAction
        {
            Fields = null,
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Act
        action.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult)!.AsObject();

        // Assert
        Assert.True(jsonResultObject.ContainsKey("fields"), "The serialized JSON should contain a 'fields' property.");
        Assert.NotNull(jsonResultObject["fields"]!.AsObject());
    }
    [Fact]
    public void UsingTargetIsProhibitedForReusableActions()
    {
        // Given
        var action = new OverlayReusableAction
        {
            Fields = new OverlayAction
            {
                Target = "Foo"
            }
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Then
        Assert.Throws<InvalidOperationException>(() => action.SerializeAsV1_2(writer));
    }

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        var json = """
        {
            "description": "Reusable description",
            "fields": {
                "description": "Test Description",
                "remove": true
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var action = OverlayV1_2Deserializer.LoadReusableAction(jsonNode, parsingContext);

        Assert.Equal("Reusable description", action.Description);
        Assert.NotNull(action.Fields);
        Assert.Equal("Test Description", action.Fields.Description);
        Assert.True(action.Fields.Remove);
    }

    [Fact]
    public void Deserialize_WithCopyField_ShouldSetCopy()
    {
        var json = """
        {
            "fields": {
                "copy": "$.paths['/pets']"
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var action = OverlayV1_2Deserializer.LoadReusableAction(jsonNode, parsingContext);

        Assert.NotNull(action.Fields);
        Assert.Equal("$.paths['/pets']", action.Fields.Copy);
    }
}