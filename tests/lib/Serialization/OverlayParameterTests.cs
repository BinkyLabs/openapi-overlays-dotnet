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
    public void SerializeAsV1_WithInlineSource_ShouldWriteCorrectJson()
    {
        // Arrange
        var parameter = new OverlayParameter
        {
            Name = "environment",
            Source = ParameterValueSource.Inline,
            Values = ["dev", "prod"]
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "name": "environment",
    "values": ["dev", "prod"]
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
    public void SerializeAsV1_WithEnvironmentSource_ShouldWriteCorrectJson()
    {
        // Arrange
        var parameter = new OverlayParameter
        {
            Name = "api_key",
            Source = ParameterValueSource.Environment,
            Separator = ","
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "name": "api_key",
    "source": "environment",
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
    public void Deserialize_WithInlineSource_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "name": "environment",
            "source": "inline",
            "values": ["dev", "staging", "prod"]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var parameter = OverlayV1Deserializer.LoadParameter(parseNode);

        // Assert
        Assert.Equal("environment", parameter.Name);
        Assert.Equal(ParameterValueSource.Inline, parameter.Source);
        Assert.NotNull(parameter.Values);
        Assert.Equal(3, parameter.Values.Count);
        Assert.Equal("dev", parameter.Values[0]);
        Assert.Equal("staging", parameter.Values[1]);
        Assert.Equal("prod", parameter.Values[2]);
    }

    [Fact]
    public void Deserialize_WithEnvironmentSource_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "name": "api_key",
            "source": "environment",
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
        Assert.Equal(ParameterValueSource.Environment, parameter.Source);
        Assert.Equal(",", parameter.Separator);
    }

    [Fact]
    public void Deserialize_DefaultsToInlineSource()
    {
        // Arrange
        var json = """
        {
            "name": "test",
            "values": ["value1"]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var parameter = OverlayV1Deserializer.LoadParameter(parseNode);

        // Assert
        Assert.Equal("test", parameter.Name);
        Assert.Equal(ParameterValueSource.Inline, parameter.Source);
    }
}

#pragma warning restore BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.