using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayComponentsV1_2Tests
{
    [Fact]
    public void SerializeAsV1_2_ShouldWriteCorrectJson()
    {
        // Arrange
        var components = new OverlayComponents
        {
            Actions = new Dictionary<string, OverlayReusableAction>
            {
                {
                    "setServerUrl",
                    new OverlayReusableAction
                    {
                        Description = "Sets the server URL",
                        Fields = new OverlayAction
                        {
                            Update = JsonNode.Parse("""
                            {
                                "url": "https://api.example.com"
                            }
                            """),
                        }
                    }
                }
            }
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "actions": {
        "setServerUrl": {
            "description": "Sets the server URL",
            "fields": {
                "update": {
                    "url": "https://api.example.com"
                }
            }
        }
    }
}
""";

        // Act
        components.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }
}