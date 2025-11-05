using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayInfoV1Tests
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
    public void SerializeAsV1_WithDescription_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayInfo = new OverlayInfo
        {
            Title = "Test Overlay",
            Version = "1.0.0",
            Description = "Test overlay description"
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "title": "Test Overlay",
    "version": "1.0.0",
    "x-description": "Test overlay description"
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
        var overlayInfo = OverlayV1Deserializer.LoadInfo(parseNode);

        // Assert
        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.0.0", overlayInfo.Version);
    }

    [Fact]
    public void Deserialize_WithDescription_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "title": "Test Overlay",
            "version": "1.0.0",
            "x-description": "Test overlay description"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayInfo = OverlayV1Deserializer.LoadInfo(parseNode);

        // Assert
        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.0.0", overlayInfo.Version);
        Assert.Equal("Test overlay description", overlayInfo.Description);
    }

    [Fact]
    public void Deserialize_WithStandardDescription_ShouldIgnoreField()
    {
        // Arrange - In V1, "description" field should be ignored, only "x-description" is recognized
        var json = """
        {
            "title": "Test Overlay",
            "version": "1.0.0",
            "description": "This should be ignored in V1"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayInfo = OverlayV1Deserializer.LoadInfo(parseNode);

        // Assert
        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.0.0", overlayInfo.Version);
        Assert.Null(overlayInfo.Description); // Should be null because "description" is not recognized in V1
    }
}