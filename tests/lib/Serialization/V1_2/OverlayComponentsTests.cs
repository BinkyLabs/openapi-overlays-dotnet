using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_2;

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

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        var json = """
        {
            "actions": {
                "setServerUrl": {
                    "fields": {
                        "target": "$.servers[0]",
                        "copy": "$.servers[1]"
                    },
                    "description": "Sets the server URL"
                }
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var components = OverlayV1_2Deserializer.LoadComponents(jsonNode, parsingContext);

        Assert.NotNull(components.Actions);
        Assert.Single(components.Actions);
        var action = components.Actions["setServerUrl"];
        Assert.Equal("Sets the server URL", action.Description);
        Assert.NotNull(action.Fields);
        Assert.Equal("$.servers[0]", action.Fields.Target);
        Assert.Equal("$.servers[1]", action.Fields.Copy);
    }
}