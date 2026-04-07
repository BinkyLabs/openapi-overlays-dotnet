using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

#pragma warning disable BOO002
public class OverlayComponentsV1Tests
{
    [Fact]
    public void SerializeAsV1_ShouldWriteCorrectJson()
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
                        Target = "$.servers[0]",
                        Update = JsonNode.Parse("""
                        {
                            "url": "https://api.example.com"
                        }
                        """),
                        Parameters =
                        [
                            new OverlayReusableActionParameter
                            {
                                Name = "region",
                                Default = "us"
                            }
                        ]
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
            "target": "$.servers[0]",
            "update": {
                "url": "https://api.example.com"
            },
            "parameters": [
                {
                    "name": "region",
                    "default": "us"
                }
            ]
        }
    }
}
""";

        // Act
        components.SerializeAsV1(writer);
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
                    "target": "$.servers[0]",
                    "update": {
                        "url": "https://api.example.com"
                    },
                    "parameters": [
                        {
                            "name": "region",
                            "default": "us"
                        }
                    ]
                }
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var components = OverlayV1Deserializer.LoadComponents(parseNode);

        // Assert
        Assert.NotNull(components.Actions);
        Assert.Single(components.Actions);
        Assert.True(components.Actions.ContainsKey("setServerUrl"));
        var action = components.Actions["setServerUrl"];
        Assert.Equal("$.servers[0]", action.Target);
        Assert.NotNull(action.Update);
        Assert.Equal("https://api.example.com", action.Update["url"]?.GetValue<string>());
        Assert.NotNull(action.Parameters);
        Assert.Single(action.Parameters);
        Assert.Equal("region", action.Parameters[0].Name);
        Assert.Equal("us", action.Parameters[0].Default);
    }

    [Fact]
    public void CombineWith_ShouldMergeActions_AndPreferLaterOnConflicts()
    {
        // Arrange
        var first = new OverlayComponents
        {
            Actions = new Dictionary<string, OverlayReusableAction>
            {
                { "setTitle", new OverlayReusableAction { Target = "$.info.title", Update = JsonNode.Parse("\"A\"") } },
                { "setVersion", new OverlayReusableAction { Target = "$.info.version", Update = JsonNode.Parse("\"1.0.0\"") } }
            }
        };
        var second = new OverlayComponents
        {
            Actions = new Dictionary<string, OverlayReusableAction>
            {
                { "setVersion", new OverlayReusableAction { Target = "$.info.version", Update = JsonNode.Parse("\"2.0.0\"") } },
                { "setDescription", new OverlayReusableAction { Target = "$.info.description", Update = JsonNode.Parse("\"desc\"") } }
            }
        };

        // Act
        var result = first.CombineWith(second);

        // Assert
        Assert.NotNull(result.Actions);
        Assert.Equal(3, result.Actions.Count);
        Assert.Equal("$.info.title", result.Actions["setTitle"].Target);
        Assert.Equal("2.0.0", result.Actions["setVersion"].Update?.GetValue<string>());
        Assert.Equal("$.info.description", result.Actions["setDescription"].Target);
    }
}
#pragma warning restore BOO002