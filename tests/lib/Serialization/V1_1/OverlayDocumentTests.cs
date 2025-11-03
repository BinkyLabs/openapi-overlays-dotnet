using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public sealed class OverlayDocumentV1_1Tests : IDisposable
{
    [Fact]
    public void SerializeAsV1_1_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Extends = "x-extends",
            Actions =
            [
                new OverlayAction
                {
                    Target = "Test Target",
                    Description = "Test Description",
                    Remove = true
                }
            ],
            Extensions = new Dictionary<string, IOverlayExtension>
            {
                { "x-custom-extension", new JsonNodeExtension(new JsonObject { { "someProperty", "someValue" } }) }
            }
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson = """
        {
            "overlay": "1.0.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "extends": "x-extends",
            "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "remove": true
                }
            ],
            "x-custom-extension": {
                "someProperty": "someValue"
            }
        }
        """;

        // Act
        overlayDocument.SerializeAsV1_1(writer);
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
            "overlay": "1.0.0",
            "info": {
                "title": "Test Overlay",
                "version": "2.0.0"
            },
            "extends": "x-extends",
            "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "remove": true
                },{
                    "target": "Test Target 2",
                    "description": "Test Description 2",
                    "remove": false
                }
            ],
            "x-custom-extension": {
                "someProperty": "someValue"
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayDocument = OverlayV1_1Deserializer.LoadDocument(parseNode);

        // Assert
        Assert.NotNull(overlayDocument);
        Assert.Equal("1.0.0", overlayDocument.Overlay);
        Assert.Equal("Test Overlay", overlayDocument.Info?.Title);
        Assert.Equal("2.0.0", overlayDocument.Info?.Version);
        Assert.Equal("x-extends", overlayDocument.Extends);
        Assert.NotNull(overlayDocument.Extensions);
        Assert.True(overlayDocument.Extensions!.ContainsKey("x-custom-extension"));
        var extensionNodeValue = Assert.IsType<JsonNodeExtension>(overlayDocument.Extensions["x-custom-extension"]);
        var extensionValue = extensionNodeValue.Node;
        var someProperty = Assert.IsType<JsonValue>(extensionValue["someProperty"], exactMatch: false);
        Assert.Equal("someValue", someProperty.GetValue<string>());

        // Assert the 2 action
        Assert.NotNull(overlayDocument.Actions);
        Assert.Equal(2, overlayDocument.Actions.Count);
        Assert.Equal("Test Target", overlayDocument.Actions[0].Target);
        Assert.Equal("Test Description", overlayDocument.Actions[0].Description);
        Assert.True(overlayDocument.Actions[0].Remove);
        Assert.Equal("Test Target 2", overlayDocument.Actions[1].Target);
        Assert.Equal("Test Description 2", overlayDocument.Actions[1].Description);
        Assert.False(overlayDocument.Actions[1].Remove);
    }

    [Fact]
    public void SerializeAsV1_1_WithUpdate_ShouldWriteCorrectJson()
    {
        // Arrange
        var updateNode = JsonNode.Parse("""
        {
            "summary": "Updated summary",
            "description": "Updated description"
        }
        """);

        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Extends = "x-extends",
            Actions =
            [
                new OverlayAction
                {
                    Target = "Test Target",
                    Description = "Test Description",
                    Update = updateNode
                }
            ]
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);


        var expectedJson = """
        {
            "overlay": "1.0.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "extends": "x-extends",
            "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "update": {
                        "summary": "Updated summary",
                        "description": "Updated description"
                    }
                }
            ]
        }
        """;

        // Act
        overlayDocument.SerializeAsV1_1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void Deserialize_WithUpdate_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "overlay": "1.0.0",
            "info": {
                "title": "Test Overlay",
                "version": "2.0.0"
            },
            "extends": "x-extends",
            "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "update": {
                        "summary": "Updated summary",
                        "description": "Updated description"
                    }
                },{
                    "target": "Test Target 2",
                    "description": "Test Description 2",
                    "update": ["tag1", "tag2"]
                }
            ]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayDocument = OverlayV1_1Deserializer.LoadDocument(parseNode);

        // Assert
        Assert.NotNull(overlayDocument);
        Assert.Equal("1.0.0", overlayDocument.Overlay);
        Assert.Equal("Test Overlay", overlayDocument.Info?.Title);
        Assert.Equal("2.0.0", overlayDocument.Info?.Version);
        Assert.Equal("x-extends", overlayDocument.Extends);

        // Assert the 2 actions
        Assert.NotNull(overlayDocument.Actions);
        Assert.Equal(2, overlayDocument.Actions.Count);

        // First action with object update
        Assert.Equal("Test Target", overlayDocument.Actions[0].Target);
        Assert.Equal("Test Description", overlayDocument.Actions[0].Description);
        var updateProperty = overlayDocument.Actions[0].Update;
        Assert.NotNull(updateProperty);
        var updateObject = updateProperty.AsObject();
        Assert.Equal("Updated summary", updateObject["summary"]?.GetValue<string>());
        Assert.Equal("Updated description", updateObject["description"]?.GetValue<string>());

        // Second action with array update
        Assert.Equal("Test Target 2", overlayDocument.Actions[1].Target);
        Assert.Equal("Test Description 2", overlayDocument.Actions[1].Description);
        var updatePropertyArray = overlayDocument.Actions[1].Update;
        Assert.NotNull(updatePropertyArray);
        var updateArray = updatePropertyArray.AsArray();
        Assert.Equal(2, updateArray.Count);
        Assert.Equal("tag1", updateArray[0]?.GetValue<string>());
        Assert.Equal("tag2", updateArray[1]?.GetValue<string>());
    }
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

    [Fact]
    public async Task Load_WithValidMemoryStream_ReturnsReadResultAsync()
    {
        // Arrange
        var json = """
        {
            "openapi": "3.0.0",
            "overlay": "1.0.0",
            "info": {
                "title": "Test Overlay",
                "version": "2.0.0"
            },
            "extends": "x-extends",
            "x-custom-extension": {
                "someProperty": "someValue"
            },
         "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "update": {
                        "summary": "Updated summary",
                        "description": "Updated description"
                    }
                },{
                    "target": "Test Target 2",
                    "description": "Test Description 2",
                    "update": ["tag1", "tag2"]
                }
            ]
        }
        """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        var (overlayDocument, _) = await OverlayDocument.LoadFromStreamAsync(stream);

        // Assert
        Assert.NotNull(overlayDocument);
        Assert.Equal("2.0.0", overlayDocument.Info?.Version);
        Assert.Equal("Test Overlay", overlayDocument.Info?.Title);
        Assert.Equal("1.0.0", overlayDocument.Overlay);
        Assert.Equal("x-extends", overlayDocument.Extends);
        Assert.NotNull(overlayDocument.Extensions);
        Assert.True(overlayDocument.Extensions.ContainsKey("x-custom-extension"));
        var extension = overlayDocument.Extensions["x-custom-extension"];
        var jsonNodeExtension = Assert.IsType<JsonNodeExtension>(extension);
        var node = jsonNodeExtension.Node;
        Assert.NotNull(node);
        Assert.Equal("someValue", node["someProperty"]?.GetValue<string>());

        // Assert the 2 actions
        Assert.NotNull(overlayDocument.Actions);
        Assert.Equal(2, overlayDocument.Actions.Count);

        // First action with object update
        Assert.Equal("Test Target", overlayDocument.Actions[0].Target);
        Assert.Equal("Test Description", overlayDocument.Actions[0].Description);
        var updateProperty = overlayDocument.Actions[0].Update;
        Assert.NotNull(updateProperty);
        var updateObject = updateProperty.AsObject();
        Assert.Equal("Updated summary", updateObject["summary"]?.GetValue<string>());
        Assert.Equal("Updated description", updateObject["description"]?.GetValue<string>());

        // Second action with array update
        Assert.Equal("Test Target 2", overlayDocument.Actions[1].Target);
        Assert.Equal("Test Description 2", overlayDocument.Actions[1].Description);
        var updatePropertyArray = overlayDocument.Actions[1].Update;
        Assert.NotNull(updatePropertyArray);
        var updateArray = updatePropertyArray.AsArray();
        Assert.Equal(2, updateArray.Count);
        Assert.Equal("tag1", updateArray[0]?.GetValue<string>());
        Assert.Equal("tag2", updateArray[1]?.GetValue<string>());

    }


    [Fact]
    public async Task LoadAsync_WithValidFilePath_ReturnsReadResult()
    {
        // Arrange
        var json = """
        {
            "openapi": "3.0.0",
            "overlay": "1.0.0",
            "info": {
                "title": "Test Overlay",
                "version": "2.0.0"
            },
            "extends": "x-extends",
            "x-custom-extension": {
                "someProperty": "someValue"
            },
         "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "update": {
                        "summary": "Updated summary",
                        "description": "Updated description"
                    }
                },{
                    "target": "Test Target 2",
                    "description": "Test Description 2",
                    "update": ["tag1", "tag2"]
                }
            ]
        }
        """;

        var tempFile = @"./ValidFile.json";
        await File.WriteAllTextAsync(tempFile, json);

        // Act
        var (overlayDocument, _) = await OverlayDocument.LoadFromUrlAsync(tempFile);

        // Assert
        Assert.NotNull(overlayDocument);
        Assert.Equal("2.0.0", overlayDocument.Info?.Version);
        Assert.Equal("Test Overlay", overlayDocument.Info?.Title);
        Assert.Equal("1.0.0", overlayDocument.Overlay);
        Assert.Equal("x-extends", overlayDocument.Extends);
        Assert.NotNull(overlayDocument.Extensions);
        Assert.True(overlayDocument.Extensions.ContainsKey("x-custom-extension"));
        var extension = overlayDocument.Extensions["x-custom-extension"];
        var jsonNodeExtension = Assert.IsType<JsonNodeExtension>(extension);
        var node = jsonNodeExtension.Node;
        Assert.NotNull(node);
        Assert.Equal("someValue", node["someProperty"]?.GetValue<string>());

        // Assert the 2 actions
        Assert.NotNull(overlayDocument.Actions);
        Assert.Equal(2, overlayDocument.Actions.Count);

        // First action with object update
        Assert.Equal("Test Target", overlayDocument.Actions[0].Target);
        Assert.Equal("Test Description", overlayDocument.Actions[0].Description);
        var updateProperty = overlayDocument.Actions[0].Update;
        Assert.NotNull(updateProperty);
        var updateObject = updateProperty.AsObject();
        Assert.Equal("Updated summary", updateObject["summary"]?.GetValue<string>());
        Assert.Equal("Updated description", updateObject["description"]?.GetValue<string>());

        // Second action with array update
        Assert.Equal("Test Target 2", overlayDocument.Actions[1].Target);
        Assert.Equal("Test Description 2", overlayDocument.Actions[1].Description);
        var updatePropertyArray = overlayDocument.Actions[1].Update;
        Assert.NotNull(updatePropertyArray);
        var updateArray = updatePropertyArray.AsArray();
        Assert.Equal(2, updateArray.Count);
        Assert.Equal("tag1", updateArray[0]?.GetValue<string>());
        Assert.Equal("tag2", updateArray[1]?.GetValue<string>());
    }

    [Fact]
    public async Task Parse_WithValidJson_ReturnsReadResultJson()
    {
        // Arrange
        var json = """
        {
            "overlay": "1.0.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "remove": true
                }
            ]
        }
        """;

        // Act
        var (overlayDocument, _) = await OverlayDocument.ParseAsync(json);

        // Assert
        Assert.NotNull(overlayDocument);
        Assert.Equal("1.0.0", overlayDocument.Overlay);
        Assert.Equal("Test Overlay", overlayDocument.Info?.Title);
        Assert.Equal("1.0.0", overlayDocument.Info?.Version);
        Assert.NotNull(overlayDocument.Actions);
        Assert.Single(overlayDocument.Actions);
        Assert.Equal("Test Target", overlayDocument.Actions[0].Target);
        Assert.Equal("Test Description", overlayDocument.Actions[0].Description);
        Assert.True(overlayDocument.Actions[0].Remove);
    }

    [Fact]
    public async Task Parse_WithValidJson_ReturnsReadResultYamlAsync()
    {
        // Arrange
        var yaml = """
        overlay: 1.0.0
        info:
          title: Test Overlay
          version: 1.0.0
        actions:
          - target: Test Target
            description: Test Description
            remove: true
        """;

        // Act
        var (overlayDocument, _) = await OverlayDocument.ParseAsync(yaml);

        // Assert
        Assert.NotNull(overlayDocument);
        Assert.Equal("1.0.0", overlayDocument.Overlay);
        Assert.Equal("Test Overlay", overlayDocument.Info?.Title);
        Assert.Equal("1.0.0", overlayDocument.Info?.Version);
        Assert.NotNull(overlayDocument.Actions);
        Assert.Single(overlayDocument.Actions);
        Assert.Equal("Test Target", overlayDocument.Actions[0].Target);
        Assert.Equal("Test Description", overlayDocument.Actions[0].Description);
        Assert.True(overlayDocument.Actions[0].Remove);
    }
    [Fact]
    public void CombineWithThrowsOnEmptyInput()
    {
        var overlayDocument = new OverlayDocument();

        Assert.Throws<ArgumentException>(() => overlayDocument.CombineWith(null!));
    }
    [Fact]
    public void UsesMetadataFromLastDocumentWhenCombiningOverlays()
    {
        // Given
        var overlayDocument1 = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Overlay 1",
                Version = "1.0.0"
            },
            Extends = "base.yaml"
        };
        var overlayDocument2 = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Overlay 2",
                Version = "1.0.1"
            },
            Extends = "base2.yaml"
        };

        // When
        var result = overlayDocument1.CombineWith(overlayDocument2);

        // Then
        Assert.Equal("Overlay 2", result.Info?.Title);
        Assert.Equal("1.0.1", result.Info?.Version);
        Assert.Equal("base2.yaml", result.Extends);
        Assert.NotNull(result.Actions);
        Assert.Empty(result.Actions);
    }
    [Fact]
    public void CombinesActionsInTheRightOrder()
    {
        // Given
        var overlayDocument1 = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction { Target = "Target1", Description = "Description1", Remove = false },
                new OverlayAction { Target = "Target2", Description = "Description2", Remove = true }
            ]
        };
        var overlayDocument2 = new OverlayDocument
        {
            Actions =
            [
                new OverlayAction { Target = "Target3", Description = "Description3", Remove = false }
            ]
        };

        // When
        var result = overlayDocument1.CombineWith(overlayDocument2);

        // Then
        Assert.NotNull(result.Actions);
        Assert.Equal(3, result.Actions.Count);
        Assert.Equal("Target1", result.Actions[0].Target);
        Assert.Equal("Description1", result.Actions[0].Description);
        Assert.False(result.Actions[0].Remove);
        Assert.Equal("Target2", result.Actions[1].Target);
        Assert.Equal("Description2", result.Actions[1].Description);
        Assert.True(result.Actions[1].Remove);
        Assert.Equal("Target3", result.Actions[2].Target);
        Assert.Equal("Description3", result.Actions[2].Description);
        Assert.False(result.Actions[2].Remove);
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

    public void Dispose()
    {
        // Cleanup
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }
}