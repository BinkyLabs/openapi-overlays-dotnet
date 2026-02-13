using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Generation;

namespace BinkyLabs.OpenApi.Overlays.Tests;

/// <summary>
/// Tests for JSONPath generation with special characters to ensure proper escaping.
/// </summary>
public class OverlayGeneratorJsonPathTests
{
    [Theory]
    [InlineData("/pets", "$.paths['/pets']")]
    [InlineData("/users/{id}", "$.paths['/users/{id}']")]
    [InlineData("~pets", "$.paths['~pets']")]
    public void Generate_WithSpecialCharactersInPropertyNames_ShouldUseBracketNotation(
        string propertyName, string expectedPathStart)
    {
        // Arrange
        var source = new JsonObject
        {
            ["openapi"] = "3.0.0",
            ["paths"] = new JsonObject
            {
                [propertyName] = new JsonObject
                {
                    ["get"] = new JsonObject
                    {
                        ["summary"] = "Old"
                    }
                }
            }
        };

        var target = new JsonObject
        {
            ["openapi"] = "3.0.0",
            ["paths"] = new JsonObject
            {
                [propertyName] = new JsonObject
                {
                    ["get"] = new JsonObject
                    {
                        ["summary"] = "New"
                    }
                }
            }
        };

        // Act
        var result = OverlayGenerator.Generate(source, target);

        // Assert
        Assert.NotNull(result.Document);
        Assert.NotEmpty(result.Document.Actions!);

        var summaryAction = result.Document.Actions!.FirstOrDefault(a => 
            a.Target!.Contains("summary"));
        
        Assert.NotNull(summaryAction);
        Assert.StartsWith(expectedPathStart, summaryAction.Target);
    }

    [Fact]
    public void Generate_WithNormalPropertyNames_ShouldUseDotNotation()
    {
        // Arrange
        var source = new JsonObject
        {
            ["openapi"] = "3.0.0",
            ["info"] = new JsonObject
            {
                ["title"] = "Old"
            }
        };

        var target = new JsonObject
        {
            ["openapi"] = "3.0.0",
            ["info"] = new JsonObject
            {
                ["title"] = "New"
            }
        };

        // Act
        var result = OverlayGenerator.Generate(source, target);

        // Assert
        Assert.NotNull(result.Document);
        Assert.NotEmpty(result.Document.Actions!);

        var titleAction = result.Document.Actions!.FirstOrDefault(a => 
            a.Target!.Contains("title"));
        
        Assert.NotNull(titleAction);
        Assert.Equal("$.info.title", titleAction.Target);
    }

    [Fact]
    public void Generate_WithMixedPropertyNames_ShouldUseCorrectNotation()
    {
        // Arrange
        var source = new JsonObject
        {
            ["openapi"] = "3.0.0",
            ["info"] = new JsonObject
            {
                ["title"] = "Old"
            },
            ["paths"] = new JsonObject
            {
                ["/pets"] = new JsonObject
                {
                    ["get"] = new JsonObject
                    {
                        ["summary"] = "Old"
                    }
                }
            }
        };

        var target = new JsonObject
        {
            ["openapi"] = "3.0.0",
            ["info"] = new JsonObject
            {
                ["title"] = "New"
            },
            ["paths"] = new JsonObject
            {
                ["/pets"] = new JsonObject
                {
                    ["get"] = new JsonObject
                    {
                        ["summary"] = "New"
                    }
                }
            }
        };

        // Act
        var result = OverlayGenerator.Generate(source, target);

        // Assert
        Assert.NotNull(result.Document);
        Assert.NotEmpty(result.Document.Actions!);

        // Normal property should use dot notation
        var titleAction = result.Document.Actions!.FirstOrDefault(a => 
            a.Target == "$.info.title");
        Assert.NotNull(titleAction);

        // Property with slash should use bracket notation
        var summaryAction = result.Document.Actions!.FirstOrDefault(a => 
            a.Target!.Contains("['/pets']"));
        Assert.NotNull(summaryAction);
    }

    [Fact]
    public void GenerateAndApply_WithEscapedPaths_ShouldWorkEndToEnd()
    {
        // Arrange - Full end-to-end test with escaped paths
        var source = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""paths"": {
                ""/api/v1/users/{id}"": {
                    ""get"": {
                        ""summary"": ""Get user""
                    }
                }
            }
        }")!;

        var target = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""paths"": {
                ""/api/v1/users/{id}"": {
                    ""get"": {
                        ""summary"": ""Get user by ID""
                    }
                }
            }
        }")!;

        // Act - Generate
        var generateResult = OverlayGenerator.Generate(source, target);
        Assert.NotNull(generateResult.Document);
        Assert.Empty(generateResult.Diagnostic!.Errors);

        // Verify JSONPath uses correct escaping for path with /
        var summaryAction = generateResult.Document.Actions!.FirstOrDefault(a => 
            a.Target!.Contains("summary"));
        Assert.NotNull(summaryAction);
        Assert.Contains("['/api/v1/users/{id}']", summaryAction.Target);

        // Act - Apply
        var sourceForApply = source.DeepClone();
        var applyDiagnostic = new Reader.OverlayDiagnostic();
        var applied = generateResult.Document.ApplyToDocument(sourceForApply, applyDiagnostic);

        // Assert - Application successful
        Assert.True(applied);
        Assert.Empty(applyDiagnostic.Errors);

        // Verify changes were applied
        var resultGet = sourceForApply["paths"]!["/api/v1/users/{id}"]!["get"] as JsonObject;
        Assert.NotNull(resultGet);
        Assert.Equal("Get user by ID", resultGet["summary"]!.GetValue<string>());
    }
}
