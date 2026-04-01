using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

#pragma warning disable BOO002
public class OverlayReusableActionParameterV1_1Tests
{
    [Fact]
    public void SerializeAsV1_1_ShouldWriteCorrectJson()
    {
        // Arrange
        var parameter = new OverlayReusableActionParameter
        {
            Name = "id"
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "name": "id"
}
""";

        // Act
        parameter.SerializeAsV1_1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_1_WithDefault_ShouldWriteCorrectJson()
    {
        // Arrange
        var parameter = new OverlayReusableActionParameter
        {
            Name = "id",
            Default = JsonNode.Parse("""
            {
                "type": "string",
                "value": "abc"
            }
            """)
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "name": "id",
    "default": {
        "type": "string",
        "value": "abc"
    }
}
""";

        // Act
        parameter.SerializeAsV1_1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_1_WhenNameIsMissing_ShouldThrow()
    {
        // Arrange
        var parameter = new OverlayReusableActionParameter();
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Act / Assert
        Assert.Throws<ArgumentNullException>(() => parameter.SerializeAsV1_1(writer));
    }

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "name": "id"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var parameter = OverlayV1_1Deserializer.LoadReusableActionParameter(parseNode);

        // Assert
        Assert.Equal("id", parameter.Name);
        Assert.Null(parameter.Default);
    }

    [Fact]
    public void Deserialize_WithDefault_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "name": "id",
            "default": {
                "type": "string",
                "value": "abc"
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var parameter = OverlayV1_1Deserializer.LoadReusableActionParameter(parseNode);

        // Assert
        Assert.Equal("id", parameter.Name);
        Assert.NotNull(parameter.Default);
        Assert.Equal("string", parameter.Default["type"]?.GetValue<string>());
        Assert.Equal("abc", parameter.Default["value"]?.GetValue<string>());
    }
}
#pragma warning restore BOO002
