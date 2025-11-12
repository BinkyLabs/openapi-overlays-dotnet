using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayInfoV1_1Tests
{
    [Fact]
    public void SerializeAsV1_1_ShouldWriteCorrectJson()
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
        overlayInfo.SerializeAsV1_1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);


        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_1_WithDescription_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayInfo = new OverlayInfo
        {
            Title = "Test Overlay",
            Version = "1.1.0",
            Description = "Test overlay description"
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "title": "Test Overlay",
    "version": "1.1.0",
    "description": "Test overlay description"
}
""";

        // Act
        overlayInfo.SerializeAsV1_1(writer);
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
            "version": "1.1.0"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);


        // Act
        var overlayInfo = OverlayV1_1Deserializer.LoadInfo(parseNode);

        // Assert
        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.1.0", overlayInfo.Version);
    }

    [Fact]
    public void Deserialize_WithDescription_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "title": "Test Overlay",
            "version": "1.1.0",
            "description": "Test overlay description"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayInfo = OverlayV1_1Deserializer.LoadInfo(parseNode);

        // Assert
        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.1.0", overlayInfo.Version);
        Assert.Equal("Test overlay description", overlayInfo.Description);
    }

    [Fact]
    public void Deserialize_WithExtensionDescription_ShouldIgnoreXDescription()
    {
        // Arrange - V1.1 should ignore x-description field
        var json = """
        {
            "title": "Test Overlay",
            "version": "1.1.0",
            "x-description": "Test overlay description via extension"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayInfo = OverlayV1_1Deserializer.LoadInfo(parseNode);

        // Assert
        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.1.0", overlayInfo.Version);
        Assert.Null(overlayInfo.Description); // x-description should be ignored in V1.1
    }

    [Fact]
    public void Deserialize_WithBothDescriptions_ShouldUseStandardDescriptionOnly()
    {
        // Arrange - When both "description" and "x-description" are present, only "description" should be used (x-description ignored)
        var json = """
        {
            "title": "Test Overlay",
            "version": "1.1.0",
            "description": "Standard description",
            "x-description": "Extension description"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayInfo = OverlayV1_1Deserializer.LoadInfo(parseNode);

        // Assert
        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.1.0", overlayInfo.Version);
        Assert.Equal("Standard description", overlayInfo.Description); // Only standard field is recognized
    }
}