using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayDocumentTests
{
    [Fact]
    public void SerializeAsV1_ShouldWriteCorrectJson()
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
            Actions = new List<OverlayAction>
            {
                new OverlayAction
                {
                    Target = "Test Target",
                    Description = "Test Description",
                    Remove = true
                }
            },
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
        overlayDocument.SerializeAsV1(writer);
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
        var overlayDocument = OverlayV1Deserializer.LoadDocument(parseNode);

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
    public void SerializeAsV1_WithUpdate_ShouldWriteCorrectJson()
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
            Actions = new List<OverlayAction>
            {
                new OverlayAction
                {
                    Target = "Test Target",
                    Description = "Test Description",
                    Update = updateNode
                }
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
                    "update": {
                        "summary": "Updated summary",
                        "description": "Updated description"
                    }
                }
            ]
        }
        """;

        // Act
        overlayDocument.SerializeAsV1(writer);
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
        var overlayDocument = OverlayV1Deserializer.LoadDocument(parseNode);

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
            Actions = new List<OverlayAction>
            {
                new OverlayAction
                {
                    Target = "Test Target",
                    Description = "Test Description",
                    Remove = true
                }
            }
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
            Actions = new List<OverlayAction>
            {
                new OverlayAction
                {
                    Target = "Test Target",
                    Description = "Test Description",
                    Remove = true
                }
            }
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
            Actions = new List<OverlayAction>
            {
                new OverlayAction
                {
                    Target = "$.info.title",
                    Description = "Test Description",
                    Remove = true
                }
            }
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
            Actions = new List<OverlayAction>
            {
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
            }
        };

        var tempUri = new Uri("http://example.com/overlay.yaml");
        var (document, overlayDiagnostic, openApiDiagnostic) = await overlayDocument.ApplyToDocumentStreamAsync(documentStream, tempUri);
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
            Actions = new List<OverlayAction>
            {
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
            }
        };

        var tempUri = new Uri("http://example.com/overlay.yaml");
        var (document, overlayDiagnostic, openApiDiagnostic) = await overlayDocument.ApplyToDocumentStreamAsync(documentStream, tempUri);
        Assert.NotNull(document);
        Assert.NotNull(overlayDiagnostic);
        Assert.NotNull(openApiDiagnostic);
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Empty(openApiDiagnostic.Errors);
        Assert.Null(document.Info.Extensions); // Title should be removed
        Assert.Equal("Updated summary", document.Paths["/test"]?.Operations?[HttpMethod.Get].Summary); // Summary should be updated

    }

    [Fact]
    public void Load_WithValidMemoryStream_ReturnsReadResult()
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
        var (overlayDocument, dignostic) = OverlayDocument.Load(stream);

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
        var (overlayDocument, dignostic) = await OverlayDocument.LoadFromUrlAsync(tempFile);

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
    public void Parse_WithValidJson_ReturnsReadResult()
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
        var (overlayDocument, dignostic) = OverlayDocument.Parse(json);

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
}