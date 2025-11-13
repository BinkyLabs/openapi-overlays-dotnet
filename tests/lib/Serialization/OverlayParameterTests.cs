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
            DefaultValues = JsonNode.Parse("""["dev", "prod"]""")
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
    public void SerializeAsV1_WithObjectDefaultValues_ShouldWriteCorrectJson()
    {
        // Arrange
        var parameter = new OverlayParameter
        {
            Name = "servers",
            DefaultValues = JsonNode.Parse("""[{"url": "https://api1.example.com"}, {"url": "https://api2.example.com"}]""")
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "name": "servers",
    "defaultValues": [{"url": "https://api1.example.com"}, {"url": "https://api2.example.com"}]
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
        var array = parameter.DefaultValues as JsonArray;
        Assert.NotNull(array);
        Assert.Equal(3, array.Count);
        Assert.Equal("dev", array[0]?.GetValue<string>());
        Assert.Equal("staging", array[1]?.GetValue<string>());
        Assert.Equal("prod", array[2]?.GetValue<string>());
    }

    [Fact]
    public void Deserialize_WithObjectDefaultValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "name": "servers",
            "defaultValues": [{"url": "https://api1.example.com"}, {"url": "https://api2.example.com"}]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var parameter = OverlayV1Deserializer.LoadParameter(parseNode);

        // Assert
        Assert.Equal("servers", parameter.Name);
        Assert.NotNull(parameter.DefaultValues);
        var array = parameter.DefaultValues as JsonArray;
        Assert.NotNull(array);
        Assert.Equal(2, array.Count);
        var obj1 = array[0] as JsonObject;
        Assert.NotNull(obj1);
        Assert.Equal("https://api1.example.com", obj1["url"]?.GetValue<string>());
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
    }

    [Fact]
    public void SerializeAsV1_WithInvalidDefaultValues_ShouldThrow()
    {
        // Arrange
        var parameter = new OverlayParameter
        {
            Name = "test",
            DefaultValues = JsonNode.Parse("""["string", 123]""") // Mixed types - invalid
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => parameter.SerializeAsV1(writer));
    }
}

#pragma warning restore BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.