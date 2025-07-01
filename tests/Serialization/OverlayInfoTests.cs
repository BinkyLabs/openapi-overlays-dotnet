using System.Text.Json.Nodes;

using Microsoft.OpenApi;
using BinkyLabs.OpenApi.Overlays.Reader.V1;
using BinkyLabs.OpenApi.Overlays.Reader;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayInfoTests
{
    [Fact]
    public void SerializeAsV1_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayInfo = new OverlayInfo
        {
            Title = "Test Overlay",
            Version = "1.0.0"
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "title": "Test Overlay",
    "version": "1.0.0"
}
""";

        // Act
        overlayInfo.SerializeAsV1(writer);
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
            "title": "Test Overlay",
            "version": "1.0.0"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);


        // Act
        var overlayInfo = OverlayV1Deserializer.LoadInfo(parseNode, new OverlayDocument());

        // Assert
        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.0.0", overlayInfo.Version);
    }
}