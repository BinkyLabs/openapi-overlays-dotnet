using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

#pragma warning disable BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayParameterTests
{
    [Fact]
    public void SerializeAsV1_WithDefaultValues_ShouldWriteCorrectJson()
    {
        // Arrange
        var parameter = new OverlayParameter
        {
            Name = "environment",
            DefaultValues = ["dev", "prod"]
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "name": "environment",
    "defaultValues": ["dev", "prod"]
}
""";

        // Act
        parameter.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_WithSeparator_ShouldWriteCorrectJson()
    {
        // Arrange
        var parameter = new OverlayParameter
        {
            Name = "api_key",
            Separator = ","
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "name": "api_key",
    "separator": ","
}
""";

        // Act
        parameter.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void Deserialize_WithDefaultValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "name": "environment",
            "defaultValues": ["dev", "staging", "prod"]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var parameter = OverlayV1Deserializer.LoadParameter(parseNode);

        // Assert
        Assert.Equal("environment", parameter.Name);
        Assert.NotNull(parameter.DefaultValues);
        Assert.Equal(3, parameter.DefaultValues.Count);
        Assert.Equal("dev", parameter.DefaultValues[0]);
        Assert.Equal("staging", parameter.DefaultValues[1]);
        Assert.Equal("prod", parameter.DefaultValues[2]);
    }

    [Fact]
    public void Deserialize_WithSeparator_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "name": "api_key",
            "separator": ","
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var parameter = OverlayV1Deserializer.LoadParameter(parseNode);

        // Assert
        Assert.Equal("api_key", parameter.Name);
        Assert.Equal(",", parameter.Separator);
    }

    [Fact]
    public void Deserialize_WithMinimalData_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "name": "test"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var parameter = OverlayV1Deserializer.LoadParameter(parseNode);

        // Assert
        Assert.Equal("test", parameter.Name);
        Assert.Null(parameter.DefaultValues);
        Assert.Null(parameter.Separator);
    }
}

#pragma warning restore BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.