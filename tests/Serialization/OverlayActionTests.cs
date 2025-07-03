using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayActionTests
{
    [Fact]
    public void SerializeAsV1_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayAction = new OverlayAction
        {
            Target = "Test Target",
            Description = "Test Description",
            Remove = true
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "target": "Test Target",
    "description": "Test Description",
    "remove": true
}
""";

        // Act
        overlayAction.SerializeAsV1(writer);
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
            "target": "Test Target",
            "description": "Test Description",
            "remove": true
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);


        // Act
        var overlayAction = OverlayV1Deserializer.LoadAction(parseNode, new OverlayDocument());

        // Assert
        Assert.Equal("Test Target", overlayAction.Target);
        Assert.Equal("Test Description", overlayAction.Description);
        Assert.True(overlayAction.Remove);
    }
}