using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_2;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayActionV1_2Tests
{
    [Fact]
    public void SerializeAsV1_2_ShouldWriteCorrectJson()
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
        overlayAction.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_2_WithUpdate_ShouldWriteCorrectJson()
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
        overlayAction.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_2_WithUpdateArray_ShouldWriteCorrectJson()
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
        overlayAction.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_2_WithCopy_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayAction = new OverlayAction
        {
            Target = "$.info.title",
            Description = "Copy description to title",
            Copy = "$.info.description"
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "target": "$.info.title",
    "description": "Copy description to title",
    "copy": "$.info.description"
}
""";

        // Act
        overlayAction.SerializeAsV1_2(writer);
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
            "target": "Test Target",
            "description": "Test Description",
            "remove": true
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var overlayAction = OverlayV1_2Deserializer.LoadAction(jsonNode, parsingContext);

        Assert.Equal("Test Target", overlayAction.Target);
        Assert.Equal("Test Description", overlayAction.Description);
        Assert.True(overlayAction.Remove);
    }

    [Fact]
    public void Deserialize_WithUpdate_ShouldSetPropertiesCorrectly()
    {
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

        var overlayAction = OverlayV1_2Deserializer.LoadAction(jsonNode, parsingContext);

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
        var json = """
        {
            "target": "Test Target",
            "description": "Test Description",
            "update": ["tag1", "tag2", "tag3"]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var overlayAction = OverlayV1_2Deserializer.LoadAction(jsonNode, parsingContext);

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
    public void Deserialize_WithCopy_ShouldSetCopy()
    {
        var json = """
        {
            "target": "$.info.title",
            "copy": "$.info.description"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var overlayAction = OverlayV1_2Deserializer.LoadAction(jsonNode, parsingContext);

        Assert.Equal("$.info.title", overlayAction.Target);
        Assert.Equal("$.info.description", overlayAction.Copy);
    }

    [Fact]
    public void Deserialize_WithNonBooleanRemove_ShouldThrow()
    {
        var json = """
        {
            "target": "Test Target",
            "remove": "true"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        Assert.Throws<InvalidOperationException>(() => OverlayV1_2Deserializer.LoadAction(jsonNode, parsingContext));
    }
}