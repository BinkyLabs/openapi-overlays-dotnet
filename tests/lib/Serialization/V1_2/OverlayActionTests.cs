using System.Text.Json.Nodes;

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
}