using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_2;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayInfoV1_2Tests
{
    [Fact]
    public void SerializeAsV1_2_ShouldWriteCorrectJson()
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
        overlayInfo.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_2_WithDescription_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayInfo = new OverlayInfo
        {
            Title = "Test Overlay",
            Version = "1.2.0",
            Description = "Test overlay description"
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "title": "Test Overlay",
    "version": "1.2.0",
    "description": "Test overlay description"
}
""";

        // Act
        overlayInfo.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        var json = """
        {
            "title": "Test Overlay",
            "version": "1.2.0"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var overlayInfo = OverlayV1_2Deserializer.LoadInfo(jsonNode, parsingContext);

        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.2.0", overlayInfo.Version);
    }

    [Fact]
    public void Deserialize_WithDescription_ShouldSetPropertiesCorrectly()
    {
        var json = """
        {
            "title": "Test Overlay",
            "version": "1.2.0",
            "description": "Test overlay description"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var overlayInfo = OverlayV1_2Deserializer.LoadInfo(jsonNode, parsingContext);

        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.2.0", overlayInfo.Version);
        Assert.Equal("Test overlay description", overlayInfo.Description);
    }

    [Fact]
    public void Deserialize_WithExtensionDescription_ShouldIgnoreXDescription()
    {
        var json = """
        {
            "title": "Test Overlay",
            "version": "1.2.0",
            "x-description": "Test overlay description via extension"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var overlayInfo = OverlayV1_2Deserializer.LoadInfo(jsonNode, parsingContext);

        Assert.Equal("Test Overlay", overlayInfo.Title);
        Assert.Equal("1.2.0", overlayInfo.Version);
        Assert.Null(overlayInfo.Description);
    }
}