using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

#pragma warning disable BOO002
public class OverlayReusableActionV1Tests
{
    [Fact]
    public void SerializeAsV1_ShouldWriteCorrectJson()
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
        action.SerializeAsV1(writer);
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
        var action = OverlayV1Deserializer.LoadReusableAction(parseNode);

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
    public void Deserialize_WithV1CopyField_ShouldSetCopy()
    {
        // Arrange
        var json = """
        {
            "target": "Test Target",
            "x-copy": "$.paths['/pets']"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var action = OverlayV1Deserializer.LoadReusableAction(parseNode);

        // Assert
        Assert.Equal("$.paths['/pets']", action.Copy);
    }

    [Fact]
    public void ResolveEnvironmentVariableValues_ShouldIgnoreUnknownAndReturnMissingRequiredSet()
    {
        // Arrange
        var stageDefault = JsonValue.Create("dev")!;
        var action = new OverlayReusableAction
        {
            EnvironmentVariables =
            [
                new OverlayReusableActionParameter { Name = "region" },
                new OverlayReusableActionParameter { Name = "stage", Default = stageDefault },
                new OverlayReusableActionParameter { Name = "tenant" }
            ]
        };
        var environmentVariableValues = new Dictionary<string, string>
        {
            ["region"] = "us",
            ["unknown"] = "x"
        };

        // Act
        var (resolvedEnvironmentVariableValues, missingRequiredEnvironmentVariableValues) =
            action.ResolveEnvironmentVariableValues(environmentVariableValues);

        // Assert
        Assert.Equal(2, resolvedEnvironmentVariableValues.Count);
        Assert.Equal("us", resolvedEnvironmentVariableValues["region"]?.GetValue<string>());
        Assert.Same(stageDefault, resolvedEnvironmentVariableValues["stage"]);
        Assert.True(missingRequiredEnvironmentVariableValues.SetEquals(["tenant"]));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1invalid")]
    [InlineData("invalid-name")]
    [InlineData("invalid_name")]
    public void ResolveEnvironmentVariableValues_WithInvalidEnvironmentVariableDefinitionName_ShouldThrow(string? definitionName)
    {
        // Arrange
        var action = new OverlayReusableAction
        {
            EnvironmentVariables =
            [
                new OverlayReusableActionParameter { Name = definitionName }
            ]
        };

        // Act + Assert
        var exception = Assert.Throws<InvalidOperationException>(() => action.ResolveEnvironmentVariableValues(new Dictionary<string, string>()));
        Assert.Contains("environment variable", exception.Message);
    }

    [Fact]
    public void ResolveEnvironmentVariableValues_WithDuplicateEnvironmentVariableDefinitionNames_ShouldThrow()
    {
        // Arrange
        var action = new OverlayReusableAction
        {
            EnvironmentVariables =
            [
                new OverlayReusableActionParameter { Name = "region" },
                new OverlayReusableActionParameter { Name = "region" }
            ]
        };

        // Act + Assert
        var exception = Assert.Throws<InvalidOperationException>(() => action.ResolveEnvironmentVariableValues(new Dictionary<string, string>()));
        Assert.Contains("Duplicate reusable action environment variable definition name 'region'.", exception.Message);
    }
}
#pragma warning restore BOO002