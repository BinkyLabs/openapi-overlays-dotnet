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
            Fields = new OverlayAction
            {
                Target = "Test Target",
                Description = "Test Description",
                Remove = true,
            },
            Parameters =
            [
                new OverlayReusableActionParameter { Name = "id" }
            ],
            EnvironmentVariables =
            [
                new OverlayReusableActionParameter { Name = "region", Default = "us" }
            ]
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "fields": {
        "target": "Test Target",
        "description": "Test Description",
        "remove": true
    },
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
            "fields": {
                "target": "Test Target",
                "description": "Test Description",
                "remove": true
            },
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
        Assert.NotNull(action.Fields);
        Assert.Equal("Test Target", action.Fields.Target);
        Assert.Equal("Test Description", action.Fields.Description);
        Assert.True(action.Fields.Remove);
        Assert.NotNull(action.Parameters);
        Assert.Single(action.Parameters);
        Assert.Equal("id", action.Parameters[0].Name);
        Assert.NotNull(action.EnvironmentVariables);
        Assert.Single(action.EnvironmentVariables);
        Assert.Equal("region", action.EnvironmentVariables[0].Name);
        Assert.Equal("us", action.EnvironmentVariables[0].Default);
    }

    [Fact]
    public void Deserialize_WithNonStringEnvironmentVariableDefault_ShouldCoerceToString()
    {
        // Arrange
        var json = """
        {
            "fields": {
                "target": "Test Target"
            },
            "environmentVariables": [
                {
                    "name": "region",
                    "default": 100
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
        Assert.NotNull(action.EnvironmentVariables);
        Assert.Single(action.EnvironmentVariables);
        Assert.Equal("100", action.EnvironmentVariables[0].Default);
        Assert.Empty(parsingContext.Diagnostic.Errors);
    }

    [Fact]
    public void Deserialize_WithV1_1CopyField_ShouldSetCopy()
    {
        // Arrange
        var json = """
        {
            "fields": {
                "target": "Test Target",
                "copy": "$.paths['/pets']"
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var action = OverlayV1_1Deserializer.LoadReusableAction(parseNode);

        // Assert
        Assert.NotNull(action.Fields);
        Assert.Equal("$.paths['/pets']", action.Fields.Copy);
    }

    [Fact]
    public void ResolveEnvironmentVariableValues_ShouldIgnoreUnknownAndReturnMissingRequiredSet()
    {
        // Arrange
        var stageDefault = "dev";
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
        Assert.Equal("us", resolvedEnvironmentVariableValues["region"]);
        Assert.Equal(stageDefault, resolvedEnvironmentVariableValues["stage"]);
        Assert.True(missingRequiredEnvironmentVariableValues.SetEquals(["tenant"]));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1invalid")]
    [InlineData("invalid-name")]
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