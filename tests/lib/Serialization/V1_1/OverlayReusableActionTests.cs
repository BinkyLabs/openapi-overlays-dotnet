using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

#pragma warning disable BOO002
public class OverlayReusableActionV1_1Tests
{
    [Fact]
    public void SerializeAsV1_1_ShouldWriteCorrectJson()
    {
        // Arrange
        var action = new OverlayReusableAction
        {
            Target = "Test Target",
            Description = "Test Description",
            Remove = true,
            Parameters =
            [
                new OverlayReusableActionParameter { Name = "id" }
            ],
            EnvironmentVariables =
            [
                new OverlayReusableActionParameter { Name = "region", Default = JsonNode.Parse("\"us\"") }
            ]
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "target": "Test Target",
    "description": "Test Description",
    "remove": true,
    "parameters": [
        {
            "name": "id"
        }
    ],
    "environmentVariables": [
        {
            "name": "region",
            "default": "us"
        }
    ]
}
""";

        // Act
        action.SerializeAsV1_1(writer);
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
            "remove": true,
            "parameters": [
                {
                    "name": "id"
                }
            ],
            "environmentVariables": [
                {
                    "name": "region",
                    "default": "us"
                }
            ]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var action = OverlayV1_1Deserializer.LoadReusableAction(parseNode);

        // Assert
        Assert.Equal("Test Target", action.Target);
        Assert.Equal("Test Description", action.Description);
        Assert.True(action.Remove);
        Assert.NotNull(action.Parameters);
        Assert.Single(action.Parameters);
        Assert.Equal("id", action.Parameters[0].Name);
        Assert.NotNull(action.EnvironmentVariables);
        Assert.Single(action.EnvironmentVariables);
        Assert.Equal("region", action.EnvironmentVariables[0].Name);
        Assert.Equal("us", action.EnvironmentVariables[0].Default?.GetValue<string>());
    }

    [Fact]
    public void Deserialize_WithV1_1CopyField_ShouldSetCopy()
    {
        // Arrange
        var json = """
        {
            "target": "Test Target",
            "copy": "$.paths['/pets']"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var action = OverlayV1_1Deserializer.LoadReusableAction(parseNode);

        // Assert
        Assert.Equal("$.paths['/pets']", action.Copy);
    }
}
#pragma warning restore BOO002