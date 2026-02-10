using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public sealed class OverlayApplyTests : IDisposable
{
    [Fact]
    public void ApplyToDocument_ShouldFailNoNode()
    {
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "Test Target",
                    Description = "Test Description",
                    Remove = true
                }
            ]
        };
        JsonNode? jsonNode = null;
        var overlayDiagnostic = new OverlayDiagnostic();
        Assert.Throws<ArgumentNullException>(() => overlayDocument.ApplyToDocument(jsonNode!, overlayDiagnostic));
    }
    [Fact]
    public void ApplyToDocument_ShouldFailNoDiagnostic()
    {
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "Test Target",
                    Description = "Test Description",
                    Remove = true
                }
            ]
        };
        var jsonNode = new JsonObject();
        OverlayDiagnostic? overlayDiagnostic = null;
        Assert.Throws<ArgumentNullException>(() => overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic!));
    }
    [Fact]
    public void ApplyToDocument_ShouldApplyTheActions()
    {
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.info.title",
                    Description = "Test Description",
                    Remove = true
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic);
        Assert.True(result, "ApplyToDocument should return true when actions are applied successfully.");
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Null(jsonNode["info"]?["title"]);
    }
    [Fact]
    public async Task ShouldApplyTheOverlayToAnOpenApiDocumentFromYaml()
    {
        var yamlDocument =
        """
        openapi: 3.1.0
        info:
          title: Test API
          version: 1.0.0
          randomProperty: randomValue
        paths:
          /test:
            get:
              summary: Test endpoint
              responses:
                '200':
                  description: OK
        """;
        var documentStream = new MemoryStream();
        using var writer = new StreamWriter(documentStream, leaveOpen: true);
        await writer.WriteAsync(yamlDocument);
        await writer.FlushAsync();
        documentStream.Seek(0, SeekOrigin.Begin);
        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.info.randomProperty",
                    Description = "Remove randomProperty",
                    Remove = true
                },
                new OverlayAction
                {
                    Target = "$.paths['/test'].get",
                    Description = "Update summary",
                    Update = new JsonObject
                    {
                        ["summary"] = "Updated summary"
                    }
                }
            ]
        };

        var tempUri = new Uri("http://example.com/overlay.yaml");
        var (document, overlayDiagnostic, openApiDiagnostic, result) = await overlayDocument.ApplyToDocumentStreamAndLoadAsync(documentStream, tempUri);
        Assert.True(result, "Overlay application should succeed.");
        Assert.NotNull(document);
        Assert.NotNull(overlayDiagnostic);
        Assert.NotNull(openApiDiagnostic);
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Empty(openApiDiagnostic.Errors);
        Assert.Null(document.Info.Extensions); // Title should be removed
        Assert.Equal("Updated summary", document.Paths["/test"]?.Operations?[HttpMethod.Get].Summary); // Summary should be updated
    }
    [Fact]
    public async Task ShouldApplyTheOverlayToAnOpenApiDocumentFromJson()
    {
        var json =
        """
        {
            "openapi": "3.1.0",
            "info": {
                "title": "Test API",
                "version": "1.0.0",
                "randomProperty": "randomValue"
            },
            "paths": {
                "/test": {
                    "get": {
                        "summary": "Test endpoint",
                        "responses": {
                            "200": {
                                "description": "OK"
                            }
                        }
                    }
                }
            }
        }
        """;
        var documentStream = new MemoryStream();
        using var writer = new StreamWriter(documentStream, leaveOpen: true);
        await writer.WriteAsync(json);
        await writer.FlushAsync();
        documentStream.Seek(0, SeekOrigin.Begin);
        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.info.randomProperty",
                    Description = "Remove randomProperty",
                    Remove = true
                },
                new OverlayAction
                {
                    Target = "$.paths['/test'].get",
                    Description = "Update summary",
                    Update = new JsonObject
                    {
                        ["summary"] = "Updated summary"
                    }
                }
            ]
        };

        var tempUri = new Uri("http://example.com/overlay.yaml");
        var (document, overlayDiagnostic, openApiDiagnostic, result) = await overlayDocument.ApplyToDocumentStreamAndLoadAsync(documentStream, tempUri);
        Assert.True(result, "Overlay application should succeed.");
        Assert.NotNull(document);
        Assert.NotNull(overlayDiagnostic);
        Assert.NotNull(openApiDiagnostic);
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Empty(openApiDiagnostic.Errors);
        Assert.Null(document.Info.Extensions); // Title should be removed
        Assert.Equal("Updated summary", document.Paths["/test"]?.Operations?[HttpMethod.Get].Summary); // Summary should be updated
    }
    private readonly string _tempFilePath = Path.ChangeExtension(Path.GetTempFileName(), ".json");
    [Fact]
    public async Task ApplyToDocumentAsync_WithRelativePath_ShouldSucceed()
    {
        // Arrange
        var openApiDocument = """
        {
            "openapi": "3.0.1",
            "info": {
                "title": "Test API",
                "version": "1.0.0"
            },
            "paths": {
                "/test": {
                    "get": {
                        "summary": "Original summary",
                        "responses": {
                            "200": {
                                "description": "Success"
                            }
                        }
                    }
                }
            }
        }
        """;

        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo { Title = "Test Overlay", Version = "1.0.0" },
            Actions = new List<OverlayAction>
            {
                new OverlayAction
                {
                    Target = "$.paths['/test'].get",
                    Description = "Update summary",
                    Update = new JsonObject
                    {
                        ["summary"] = "Updated summary"
                    }
                }
            }
        };

        // Create a temporary file with a relative path
        await File.WriteAllTextAsync(_tempFilePath, openApiDocument);

        // Act
        var (document, overlayDiagnostic, openApiDiagnostic, result) = await overlayDocument.ApplyToDocumentAndLoadAsync(_tempFilePath);

        // Assert
        Assert.True(result, "Overlay application should succeed.");
        Assert.NotNull(document);
        Assert.NotNull(overlayDiagnostic);
        Assert.NotNull(openApiDiagnostic);
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Empty(openApiDiagnostic.Errors);
        Assert.Equal("Updated summary", document.Paths["/test"]?.Operations?.Values?.FirstOrDefault()?.Summary);
    }
    [Fact]
    public async Task ApplyToDocumentAsync_ContinuesToTheNextActionWhenOneFails()
    {
        // Arrange
        var openApiDocument = """
        {
            "openapi": "3.0.1",
            "info": {
                "title": "Test API",
                "version": "1.0.0"
            },
            "paths": {
                "/test": {
                    "get": {
                        "summary": "Original summary",
                        "responses": {
                            "200": {
                                "description": "Success"
                            }
                        }
                    }
                }
            }
        }
        """;

        // Create a temporary file with a relative path
        await File.WriteAllTextAsync(_tempFilePath, openApiDocument);

        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo { Title = "Test Overlay", Version = "1.0.0" },
            Actions = new List<OverlayAction>
            {
                new OverlayAction
                {
                    Target = "$$$$.paths['/nonexistent'].get",
                    Description = "This action will fail",
                    Update = new JsonObject
                    {
                        ["summary"] = "Should not be applied"
                    }
                },
                new OverlayAction
                {
                    Target = "$.paths['/test'].get",
                    Description = "Update summary",
                    Update = new JsonObject
                    {
                        ["summary"] = "Updated summary"
                    }
                }
            }
        };

        // Act
        var (document, overlayDiagnostic, openApiDiagnostic, result) = await overlayDocument.ApplyToDocumentAndLoadAsync(_tempFilePath);
        // Assert
        Assert.False(result, "Overlay application should fail.");
        Assert.NotNull(document);
        Assert.NotNull(overlayDiagnostic);
        Assert.NotNull(openApiDiagnostic);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Empty(openApiDiagnostic.Errors);
        Assert.Equal("Updated summary", document.Paths["/test"]?.Operations?.Values?.FirstOrDefault()?.Summary);
    }
    [Fact]
    public async Task UpdatesExistingArrayAppendsEntries()
    {
        // Given
        // Arrange
        var openApiDocument = """
        {
            "openapi": "3.0.1",
            "info": {
                "title": "Test API",
                "version": "1.0.0"
            },
            "paths": {
                "/test": {
                    "get": {
                        "summary": "Original summary",
                        "responses": {
                            "200": {
                                "description": "Success"
                            }
                        },
                        "tags": ["foo"]
                    }
                }
            }
        }
        """;

        // Create a temporary file with a relative path
        await File.WriteAllTextAsync(_tempFilePath, openApiDocument);
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.paths['/test'].get",
                    Description = "Append to existing array",
                    Update = new JsonObject
                    {
                        ["tags"] = new JsonArray("bar", "buzz")
                    }
                }
            ]
        };

        // When
        var (document, overlayDiagnostic, openApiDiagnostic, result) = await overlayDocument.ApplyToDocumentAndLoadAsync(_tempFilePath);

        // Then
        Assert.True(result, "Overlay application should succeed.");
        Assert.NotNull(document);
        Assert.NotNull(overlayDiagnostic);
        Assert.NotNull(openApiDiagnostic);
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Empty(openApiDiagnostic.Errors);
        var tags = document.Paths["/test"]?.Operations?[HttpMethod.Get].Tags;
        Assert.NotNull(tags);
        Assert.Equal(3, tags.Count);
        var tagsAsArray = tags.Select(t => t.Name).ToArray();
        Assert.Contains("foo", tagsAsArray);
        Assert.Contains("bar", tagsAsArray);
        Assert.Contains("buzz", tagsAsArray);
    }
    [Fact]
    public void ApplyToDocument_ShouldWarnWhenRemoveTargetMatchesZeroNodes()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.paths['/nonexistent'].get",
                    Description = "Remove non-existent path",
                    Remove = true
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            },
            ["paths"] = new JsonObject
            {
                ["/test"] = new JsonObject
                {
                    ["get"] = new JsonObject
                    {
                        ["summary"] = "Test endpoint"
                    }
                }
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic);

        // Assert
        Assert.True(result, "ApplyToDocument should return true even when no nodes match.");
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Single(overlayDiagnostic.Warnings);
        Assert.Contains("Target '$.paths['/nonexistent'].get' matched 0 nodes", overlayDiagnostic.Warnings[0].Message);
    }

    [Fact]
    public void ApplyToDocument_ShouldWarnWhenUpdateTargetMatchesZeroNodes()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.info.nonexistentField",
                    Description = "Update non-existent field",
                    Update = new JsonObject
                    {
                        ["value"] = "test"
                    }
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic);

        // Assert
        Assert.True(result, "ApplyToDocument should return true even when no nodes match.");
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Single(overlayDiagnostic.Warnings);
        Assert.Contains("Target '$.info.nonexistentField' matched 0 nodes", overlayDiagnostic.Warnings[0].Message);
    }

    [Fact]
    public void ApplyToDocument_ShouldWarnWhenCopyTargetMatchesZeroNodes()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.paths['/nonexistent'].get",
                    Description = "Copy from non-existent path",
                    Copy = "$.info.title"
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            },
            ["paths"] = new JsonObject
            {
                ["/test"] = new JsonObject
                {
                    ["get"] = new JsonObject
                    {
                        ["summary"] = "Test endpoint"
                    }
                }
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic);

        // Assert
        Assert.True(result, "ApplyToDocument should return true even when no nodes match.");
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Single(overlayDiagnostic.Warnings);
        Assert.Contains("Target '$.paths['/nonexistent'].get' matched 0 nodes", overlayDiagnostic.Warnings[0].Message);
    }

    [Fact]
    public void ApplyToDocument_ShouldNotWarnWhenTargetMatchesNodes()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.info",
                    Description = "Update existing info object",
                    Update = new JsonObject
                    {
                        ["description"] = "Updated Description"
                    }
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic);

        // Assert
        Assert.True(result, "ApplyToDocument should return true when nodes match.");
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Empty(overlayDiagnostic.Warnings);
    }

    [Fact]
    public void ApplyToDocument_ShouldWarnForZeroMatchesAndSucceedForOthers()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.info.nonexistent",
                    Description = "Update non-existent field",
                    Update = new JsonObject
                    {
                        ["value"] = "test"
                    }
                },
                new OverlayAction
                {
                    Target = "$.info",
                    Description = "Update existing field",
                    Update = new JsonObject
                    {
                        ["description"] = "Updated Description"
                    }
                },
                new OverlayAction
                {
                    Target = "$.paths.nonexistent",
                    Description = "Remove non-existent path",
                    Remove = true
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic);

        // Assert
        Assert.True(result, "ApplyToDocument should return true overall.");
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Equal(2, overlayDiagnostic.Warnings.Count);
        Assert.Contains("Target '$.info.nonexistent' matched 0 nodes", overlayDiagnostic.Warnings[0].Message);
        Assert.Contains("Target '$.paths.nonexistent' matched 0 nodes", overlayDiagnostic.Warnings[1].Message);
        // Verify the successful action was applied
        Assert.Equal("Updated Description", jsonNode["info"]?["description"]?.ToString());
    }
    [Fact]
    public void ApplyToDocument_StrictMode_ShouldErrorWhenRemoveTargetMatchesZeroNodes()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.paths['/nonexistent'].get",
                    Description = "Remove non-existent path",
                    Remove = true
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            },
            ["paths"] = new JsonObject
            {
                ["/test"] = new JsonObject
                {
                    ["get"] = new JsonObject
                    {
                        ["summary"] = "Test endpoint"
                    }
                }
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic, strict: true);

        // Assert
        Assert.False(result, "ApplyToDocument should return false in strict mode when no nodes match.");
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Contains("Target '$.paths['/nonexistent'].get' matched 0 nodes", overlayDiagnostic.Errors[0].Message);
        Assert.Empty(overlayDiagnostic.Warnings);
    }

    [Fact]
    public void ApplyToDocument_StrictMode_ShouldErrorWhenUpdateTargetMatchesZeroNodes()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.info.nonexistentField",
                    Description = "Update non-existent field",
                    Update = new JsonObject
                    {
                        ["value"] = "test"
                    }
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic, strict: true);

        // Assert
        Assert.False(result, "ApplyToDocument should return false in strict mode when no nodes match.");
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Contains("Target '$.info.nonexistentField' matched 0 nodes", overlayDiagnostic.Errors[0].Message);
        Assert.Empty(overlayDiagnostic.Warnings);
    }

    [Fact]
    public void ApplyToDocument_StrictMode_ShouldErrorWhenCopyTargetMatchesZeroNodes()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.paths['/nonexistent'].get",
                    Description = "Copy from non-existent path",
                    Copy = "$.info.title"
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            },
            ["paths"] = new JsonObject
            {
                ["/test"] = new JsonObject
                {
                    ["get"] = new JsonObject
                    {
                        ["summary"] = "Test endpoint"
                    }
                }
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic, strict: true);

        // Assert
        Assert.False(result, "ApplyToDocument should return false in strict mode when no nodes match.");
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Contains("Target '$.paths['/nonexistent'].get' matched 0 nodes", overlayDiagnostic.Errors[0].Message);
        Assert.Empty(overlayDiagnostic.Warnings);
    }

    [Fact]
    public void ApplyToDocument_StrictModeDisabled_ShouldWarnWhenTargetMatchesZeroNodes()
    {
        // Arrange - same scenario as strict mode, but with strict=false (default)
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.paths['/nonexistent'].get",
                    Description = "Remove non-existent path",
                    Remove = true
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            },
            ["paths"] = new JsonObject
            {
                ["/test"] = new JsonObject
                {
                    ["get"] = new JsonObject
                    {
                        ["summary"] = "Test endpoint"
                    }
                }
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic, strict: false);

        // Assert
        Assert.True(result, "ApplyToDocument should return true when strict mode is disabled.");
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Single(overlayDiagnostic.Warnings);
        Assert.Contains("Target '$.paths['/nonexistent'].get' matched 0 nodes", overlayDiagnostic.Warnings[0].Message);
    }

    [Fact]
    public void ApplyToDocument_StrictMode_ShouldStopAtFirstErrorButContinueOtherActions()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.info.nonexistent",
                    Description = "Update non-existent field",
                    Update = new JsonObject
                    {
                        ["value"] = "test"
                    }
                },
                new OverlayAction
                {
                    Target = "$.info",
                    Description = "Update existing field",
                    Update = new JsonObject
                    {
                        ["description"] = "Updated Description"
                    }
                },
                new OverlayAction
                {
                    Target = "$.paths.nonexistent",
                    Description = "Remove non-existent path",
                    Remove = true
                }
            ]
        };
        var jsonNode = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["title"] = "Test Title",
                ["version"] = "1.0.0"
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(jsonNode, overlayDiagnostic, strict: true);

        // Assert
        Assert.False(result, "ApplyToDocument should return false in strict mode when any action has zero matches.");
        Assert.Equal(2, overlayDiagnostic.Errors.Count);
        Assert.Contains("Target '$.info.nonexistent' matched 0 nodes", overlayDiagnostic.Errors[0].Message);
        Assert.Contains("Target '$.paths.nonexistent' matched 0 nodes", overlayDiagnostic.Errors[1].Message);
        Assert.Empty(overlayDiagnostic.Warnings);
        // Verify the successful action was still applied
        Assert.Equal("Updated Description", jsonNode["info"]?["description"]?.ToString());
    }

    public void Dispose()
    {
        // Cleanup
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }
}