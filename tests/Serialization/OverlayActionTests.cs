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
    public void SerializeAsV1_WithUpdate_ShouldWriteCorrectJson()
    {
        // Arrange
        var updateNode = JsonNode.Parse("""
        {
            "summary": "Updated summary",
            "description": "Updated description",
            "operationId": "updateOperation"
        }
        """);

        var overlayAction = new OverlayAction
        {
            Target = "Test Target",
            Description = "Test Description",
            Remove = false,
            Update = updateNode
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "target": "Test Target",
    "description": "Test Description",
    "update": {
        "summary": "Updated summary",
        "description": "Updated description",
        "operationId": "updateOperation"
    }
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
    public void SerializeAsV1_WithUpdateArray_ShouldWriteCorrectJson()
    {
        // Arrange
        var updateNode = JsonNode.Parse("""
        ["tag1", "tag2", "tag3"]
        """);

        var overlayAction = new OverlayAction
        {
            Target = "Test Target",
            Description = "Test Description",
            Update = updateNode
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "target": "Test Target",
    "description": "Test Description",
    "update": ["tag1", "tag2", "tag3"]
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

    [Fact]
    public void Deserialize_WithUpdate_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "target": "Test Target",
            "description": "Test Description",
            "remove": false,
            "update": {
                "summary": "Updated summary",
                "description": "Updated description",
                "operationId": "updateOperation"
            }
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
        Assert.False(overlayAction.Remove);
        Assert.NotNull(overlayAction.Update);

        var updateObject = overlayAction.Update.AsObject();
        Assert.Equal("Updated summary", updateObject["summary"]?.GetValue<string>());
        Assert.Equal("Updated description", updateObject["description"]?.GetValue<string>());
        Assert.Equal("updateOperation", updateObject["operationId"]?.GetValue<string>());
    }

    [Fact]
    public void Deserialize_WithUpdateArray_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "target": "Test Target",
            "description": "Test Description",
            "update": ["tag1", "tag2", "tag3"]
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
        Assert.NotNull(overlayAction.Update);

        var updateArray = overlayAction.Update.AsArray();
        Assert.Equal(3, updateArray.Count);
        Assert.Equal("tag1", updateArray[0]?.GetValue<string>());
        Assert.Equal("tag2", updateArray[1]?.GetValue<string>());
        Assert.Equal("tag3", updateArray[2]?.GetValue<string>());
    }

    [Fact]
    public void Deserialize_WithUpdatePrimitiveValue_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "target": "Test Target",
            "description": "Test Description",
            "update": "simple string value"
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
        Assert.NotNull(overlayAction.Update);
        Assert.Equal("simple string value", overlayAction.Update.GetValue<string>());
    }
}