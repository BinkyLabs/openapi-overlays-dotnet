using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

#pragma warning disable BOO002
public class OverlayComponentsV1_1Tests
{
    [Fact]
    public void SerializeAsV1_1_ShouldWriteCorrectJson()
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
                            Target = "$.servers[0]",
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
                "target": "$.servers[0]",
                "update": {
                    "url": "https://api.example.com"
                }
            }
        }
    }
}
""";

        // Act
        components.SerializeAsV1_1(writer);
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
            "actions": {
                "setServerUrl": {
                    "fields": {
                        "target": "$.servers[0]",
                        "update": {
                            "url": "https://api.example.com"
                        }
                    },
                    "description": "Sets the server URL"
                }
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var components = OverlayV1_1Deserializer.LoadComponents(parseNode);

        // Assert
        Assert.NotNull(components.Actions);
        Assert.Single(components.Actions);
        Assert.True(components.Actions.ContainsKey("setServerUrl"));
        var action = components.Actions["setServerUrl"];
        Assert.NotNull(action.Fields);
        Assert.Equal("$.servers[0]", action.Fields.Target);
        Assert.NotNull(action.Fields.Update);
        Assert.Equal("https://api.example.com", action.Fields.Update["url"]?.GetValue<string>());
        Assert.Equal("Sets the server URL", action.Description);
    }

    [Fact]
    public void CombineWith_ShouldMergeActions_AndPreferLaterOnConflicts()
    {
        // Arrange
        var first = new OverlayComponents
        {
            Actions = new Dictionary<string, OverlayReusableAction>
            {
                { "setTitle", new OverlayReusableAction { Fields = new OverlayAction { Target = "$.info.title", Update = JsonNode.Parse("\"A\"") } } },
                { "setVersion", new OverlayReusableAction { Fields = new OverlayAction { Target = "$.info.version", Update = JsonNode.Parse("\"1.0.0\"") } } }
            }
        };
        var second = new OverlayComponents
        {
            Actions = new Dictionary<string, OverlayReusableAction>
            {
                { "setVersion", new OverlayReusableAction { Fields = new OverlayAction { Target = "$.info.version", Update = JsonNode.Parse("\"2.0.0\"") } } },
                { "setDescription", new OverlayReusableAction { Fields = new OverlayAction { Target = "$.info.description", Update = JsonNode.Parse("\"desc\"") } } }
            }
        };

        // Act
        var result = first.CombineWith(second);

        // Assert
        Assert.NotNull(result.Actions);
        Assert.Equal(3, result.Actions.Count);
        Assert.Equal("$.info.title", result.Actions["setTitle"].Fields?.Target);
        Assert.Equal("2.0.0", result.Actions["setVersion"].Fields?.Update?.GetValue<string>());
        Assert.Equal("$.info.description", result.Actions["setDescription"].Fields?.Target);
    }
}
#pragma warning restore BOO002