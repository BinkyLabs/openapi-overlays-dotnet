using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class UpgradeV1_1Tests
{
    [Fact]
    public async Task UpgradesAV1DocumentToV1_1Async()
    {
        // Given
        var inputJson =
        """
        {
            "overlay": "1.0.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "actions": [
                {
                    "target": "$.info.title",
                    "description": "Copy description to title",
                    "x-copy": "$.info.description"
                }
            ]
        }
        """;
        // When
        var (document, _) = await OverlayDocument.ParseAsync(inputJson);
        Assert.NotNull(document);

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);
        document.SerializeAsV1_1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(inputJson);
        expectedJsonObject!["overlay"] = "1.1.0";
        expectedJsonObject!["actions"]![0]!["copy"] = expectedJsonObject["actions"]![0]!["x-copy"]!.DeepClone();
        expectedJsonObject["actions"]![0]!.AsObject().Remove("x-copy");

        // Then
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The upgraded JSON does not match the expected JSON.");
    }
    [Fact]
    public async Task DowngradesAV1_1DocumentToV1_0Async()
	{
        // Given
        var inputJson =
        """
        {
            "overlay": "1.1.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.1.0"
            },
            "actions": [
                {
                    "target": "$.info.title",
                    "description": "Copy description to title",
                    "copy": "$.info.description"
                }
            ]
        }
        """;
        // When
        var (document, _) = await OverlayDocument.ParseAsync(inputJson);
        Assert.NotNull(document);

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);
        document.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(inputJson);
        expectedJsonObject!["overlay"] = "1.0.0";
        expectedJsonObject!["actions"]![0]!["x-copy"] = expectedJsonObject["actions"]![0]!["copy"]!.DeepClone();
        expectedJsonObject["actions"]![0]!.AsObject().Remove("copy");

        // Then
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The downgraded JSON does not match the expected JSON.");
	}
}