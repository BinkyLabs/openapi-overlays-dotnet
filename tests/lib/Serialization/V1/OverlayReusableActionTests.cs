using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

#pragma warning disable BOO002
public class OverlayReusableActionV1Tests
{
    [Fact]
    public void SerializeAsV1_ShouldWriteCorrectJson()
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
        action.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_WithoutDescription_ShouldOmitDescription()
    {
        // Arrange
        var action = new OverlayReusableAction
        {
            Fields = new OverlayAction
            {
                Remove = true,
            }
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Act
        action.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult)!.AsObject();

        // Assert
        Assert.False(jsonResultObject.ContainsKey("description"), "Description should not be serialized when null.");
    }

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        // Arrange
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
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var action = OverlayV1Deserializer.LoadReusableAction(parseNode);

        // Assert
        Assert.Equal("Reusable description", action.Description);
        Assert.NotNull(action.Fields);
        Assert.Equal("Test Description", action.Fields.Description);
        Assert.True(action.Fields.Remove);
    }
}
#pragma warning restore BOO002