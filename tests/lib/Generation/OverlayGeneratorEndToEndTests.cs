using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Generation;

namespace BinkyLabs.OpenApi.Overlays.Tests;

/// <summary>
/// End-to-end tests for overlay generation and application to ensure generated overlays
/// correctly transform source documents into target documents.
/// </summary>
public class OverlayGeneratorEndToEndTests
{
    [Fact]
    public void GenerateAndApply_WithArrayReplacement_ShouldReplaceNotAppend()
    {
        // Arrange
        var source = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""servers"": [
                { ""url"": ""https://api.example.com/v1"" }
            ]
        }")!;

        var target = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""servers"": [
                { ""url"": ""https://api.example.com/v2"" }
            ]
        }")!;

        // Act - Generate overlay
        var generateResult = OverlayGenerator.Generate(source, target);

        // Assert - Overlay generated successfully
        Assert.NotNull(generateResult.Document);
        Assert.NotNull(generateResult.Diagnostic);
        Assert.Empty(generateResult.Diagnostic.Errors);
        Assert.NotNull(generateResult.Document.Actions);

        // Apply overlay to source
        var sourceForApply = source.DeepClone();
        var applyDiagnostic = new Reader.OverlayDiagnostic();
        var applied = generateResult.Document.ApplyToDocument(sourceForApply, applyDiagnostic);

        // Assert - Application successful
        Assert.True(applied);
        Assert.Empty(applyDiagnostic.Errors);

        // Assert - Result matches target (array was replaced, not appended)
        var resultServers = sourceForApply["servers"] as JsonArray;
        var targetServers = target["servers"] as JsonArray;
        
        Assert.NotNull(resultServers);
        Assert.NotNull(targetServers);
        Assert.Equal(targetServers.Count, resultServers.Count); // Should be 1, not 2
        Assert.True(JsonNode.DeepEquals(targetServers, resultServers));
    }

    [Fact]
    public void GenerateAndApply_WithScalarValueUpdate_ShouldReplaceValue()
    {
        // Arrange
        var source = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Old API"",
                ""version"": ""1.0.0""
            }
        }")!;

        var target = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""New API"",
                ""version"": ""2.0.0""
            }
        }")!;

        // Act - Generate overlay
        var generateResult = OverlayGenerator.Generate(source, target);

        // Assert - Overlay generated
        Assert.NotNull(generateResult.Document);
        Assert.Empty(generateResult.Diagnostic!.Errors);

        // Apply overlay
        var sourceForApply = source.DeepClone();
        var applyDiagnostic = new Reader.OverlayDiagnostic();
        var applied = generateResult.Document.ApplyToDocument(sourceForApply, applyDiagnostic);

        // Assert - Values were replaced
        Assert.True(applied);
        Assert.Empty(applyDiagnostic.Errors);
        
        var resultInfo = sourceForApply["info"] as JsonObject;
        Assert.NotNull(resultInfo);
        Assert.Equal("New API", resultInfo["title"]!.GetValue<string>());
        Assert.Equal("2.0.0", resultInfo["version"]!.GetValue<string>());
    }

    [Fact]
    public void GenerateAndApply_WithPathsStartingWithSlash_ShouldUseCorrectJsonPath()
    {
        // Arrange
        var source = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""paths"": {
                ""/pets"": {
                    ""get"": {
                        ""summary"": ""List pets""
                    }
                }
            }
        }")!;

        var target = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""paths"": {
                ""/pets"": {
                    ""get"": {
                        ""summary"": ""List all pets""
                    }
                }
            }
        }")!;

        // Act - Generate overlay
        var generateResult = OverlayGenerator.Generate(source, target);

        // Assert - Overlay uses bracket notation for path with /
        Assert.NotNull(generateResult.Document);
        Assert.NotNull(generateResult.Diagnostic);
        Assert.Empty(generateResult.Diagnostic.Errors);
        Assert.NotEmpty(generateResult.Document.Actions!);
        
        var summaryAction = generateResult.Document.Actions!.FirstOrDefault(a => 
            a.Target!.Contains("summary"));
        Assert.NotNull(summaryAction);
        
        // Should use bracket notation: $.paths['/pets'].get.summary
        Assert.Contains("['/pets']", summaryAction.Target);

        // Apply overlay
        var sourceForApply = source.DeepClone();
        var applyDiagnostic = new Reader.OverlayDiagnostic();
        var applied = generateResult.Document.ApplyToDocument(sourceForApply, applyDiagnostic);

        // Assert - Summary was updated
        Assert.True(applied);
        Assert.Empty(applyDiagnostic.Errors);
        
        var resultPaths = sourceForApply["paths"] as JsonObject;
        Assert.NotNull(resultPaths);
        var resultPetsPath = resultPaths["/pets"] as JsonObject;
        Assert.NotNull(resultPetsPath);
        var resultGet = resultPetsPath["get"] as JsonObject;
        Assert.NotNull(resultGet);
        var resultSummary = resultGet["summary"];
        Assert.NotNull(resultSummary);
        Assert.Equal("List all pets", resultSummary.GetValue<string>());
    }

    [Fact]
    public void GenerateAndApply_WithComplexDifferences_ShouldProduceExactMatch()
    {
        // Arrange - A more complex scenario
        var source = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Pet Store"",
                ""version"": ""1.0.0""
            },
            ""servers"": [
                { ""url"": ""https://old.example.com"" }
            ],
            ""paths"": {
                ""/pets"": {
                    ""get"": {
                        ""summary"": ""Get pets"",
                        ""parameters"": [
                            { ""name"": ""limit"", ""in"": ""query"" }
                        ]
                    }
                }
            }
        }")!;

        var target = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""Pet Store API"",
                ""version"": ""2.0.0"",
                ""contact"": {
                    ""email"": ""support@example.com""
                }
            },
            ""servers"": [
                { ""url"": ""https://new.example.com"" }
            ],
            ""paths"": {
                ""/pets"": {
                    ""get"": {
                        ""summary"": ""List all pets"",
                        ""parameters"": [
                            { ""name"": ""limit"", ""in"": ""query"" },
                            { ""name"": ""offset"", ""in"": ""query"" }
                        ]
                    }
                }
            }
        }")!;

        // Act
        var generateResult = OverlayGenerator.Generate(source, target);
        Assert.NotNull(generateResult.Document);
        Assert.Empty(generateResult.Diagnostic!.Errors);

        var sourceForApply = source.DeepClone();
        var applyDiagnostic = new Reader.OverlayDiagnostic();
        var applied = generateResult.Document.ApplyToDocument(sourceForApply, applyDiagnostic);

        // Assert
        Assert.True(applied);
        Assert.Empty(applyDiagnostic.Errors);

        // Verify all key differences were applied correctly
        var resultInfo = sourceForApply["info"] as JsonObject;
        Assert.Equal("Pet Store API", resultInfo!["title"]!.GetValue<string>());
        Assert.Equal("2.0.0", resultInfo["version"]!.GetValue<string>());
        Assert.NotNull(resultInfo["contact"]);

        var resultServers = sourceForApply["servers"] as JsonArray;
        Assert.Single(resultServers!); // Should be replaced, not appended
        Assert.Equal("https://new.example.com", resultServers[0]!["url"]!.GetValue<string>());

        var resultSummary = sourceForApply["paths"]!["/pets"]!["get"]!["summary"]!.GetValue<string>();
        Assert.Equal("List all pets", resultSummary);

        var resultParams = sourceForApply["paths"]!["/pets"]!["get"]!["parameters"] as JsonArray;
        Assert.Equal(2, resultParams!.Count); // Should be replaced to 2, not kept as 1
    }

    [Fact]
    public void GenerateAndApply_WithPropertyRemoval_ShouldRemoveProperty()
    {
        // Arrange
        var source = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""API"",
                ""version"": ""1.0.0"",
                ""description"": ""To be removed""
            }
        }")!;

        var target = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""info"": {
                ""title"": ""API"",
                ""version"": ""1.0.0""
            }
        }")!;

        // Act
        var generateResult = OverlayGenerator.Generate(source, target);
        Assert.NotNull(generateResult.Document);

        var sourceForApply = source.DeepClone();
        var applyDiagnostic = new Reader.OverlayDiagnostic();
        var applied = generateResult.Document.ApplyToDocument(sourceForApply, applyDiagnostic);

        // Assert
        Assert.True(applied);
        Assert.Empty(applyDiagnostic.Errors);

        var resultInfo = sourceForApply["info"] as JsonObject;
        Assert.NotNull(resultInfo);
        Assert.False(resultInfo.ContainsKey("description"));
    }

    [Fact]
    public void GenerateAndApply_WithNestedObjectChanges_ShouldMergeCorrectly()
    {
        // Arrange
        var source = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""components"": {
                ""schemas"": {
                    ""Pet"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""name"": { ""type"": ""string"" }
                        }
                    }
                }
            }
        }")!;

        var target = JsonNode.Parse(@"{
            ""openapi"": ""3.0.0"",
            ""components"": {
                ""schemas"": {
                    ""Pet"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""name"": { 
                                ""type"": ""string"",
                                ""minLength"": 1,
                                ""maxLength"": 100
                            },
                            ""age"": { ""type"": ""integer"" }
                        }
                    }
                }
            }
        }")!;

        // Act
        var generateResult = OverlayGenerator.Generate(source, target);
        Assert.NotNull(generateResult.Document);

        var sourceForApply = source.DeepClone();
        var applyDiagnostic = new Reader.OverlayDiagnostic();
        var applied = generateResult.Document.ApplyToDocument(sourceForApply, applyDiagnostic);

        // Assert
        Assert.True(applied);
        Assert.Empty(applyDiagnostic.Errors);

        var resultName = sourceForApply["components"]!["schemas"]!["Pet"]!["properties"]!["name"] as JsonObject;
        Assert.NotNull(resultName);
        Assert.Equal(1, resultName["minLength"]!.GetValue<int>());
        Assert.Equal(100, resultName["maxLength"]!.GetValue<int>());

        var resultAge = sourceForApply["components"]!["schemas"]!["Pet"]!["properties"]!["age"];
        Assert.NotNull(resultAge);
    }

    [Fact]
    public void Generate_WithMismatchedOpenApiVersions_ShouldReturnError()
    {
        // Arrange
        var source = JsonNode.Parse(@"{ ""openapi"": ""3.0.0"", ""info"": {} }")!;
        var target = JsonNode.Parse(@"{ ""openapi"": ""3.1.0"", ""info"": {} }")!;

        // Act
        var result = OverlayGenerator.Generate(source, target);

        // Assert
        Assert.Null(result.Document);
        Assert.NotNull(result.Diagnostic);
        Assert.NotEmpty(result.Diagnostic.Errors);
        Assert.Contains(result.Diagnostic.Errors, e => 
            e.Message.Contains("3.0.0") && e.Message.Contains("3.1.0"));
    }
}
