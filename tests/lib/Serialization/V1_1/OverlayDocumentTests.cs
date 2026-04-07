using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public sealed class OverlayDocumentV1_1Tests
{
    [Fact]
#pragma warning disable BOO002
    public void SerializeAsV1_1_WithComponents_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Components = new OverlayComponents
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
                            """)
                        }
                    }
                }
            }
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson = """
        {
            "overlay": "1.1.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "x-components": {
                "actions": {
                    "setServerUrl": {
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
        overlayDocument.SerializeAsV1_1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }
#pragma warning restore BOO002

    [Fact]
#pragma warning disable BOO002
    public void Deserialize_WithComponents_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "overlay": "1.1.0",
            "info": {
                "title": "Test Overlay",
                "version": "2.0.0"
            },
            "x-components": {
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
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayDocument = OverlayV1_1Deserializer.LoadDocument(parseNode);

        // Assert
        Assert.NotNull(overlayDocument.Components);
        Assert.NotNull(overlayDocument.Components.Actions);
        Assert.Single(overlayDocument.Components.Actions);
        Assert.True(overlayDocument.Components.Actions.ContainsKey("setServerUrl"));
        var action = overlayDocument.Components.Actions["setServerUrl"];
        Assert.Equal("$.servers[0]", action.Target);
        Assert.NotNull(action.Update);
        Assert.Equal("https://api.example.com", action.Update["url"]?.GetValue<string>());
        Assert.NotNull(action.Parameters);
        Assert.Single(action.Parameters);
        Assert.Equal("region", action.Parameters[0].Name);
        Assert.Equal("us", action.Parameters[0].Default);
    }
#pragma warning restore BOO002

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
            "overlay": "1.1.0",
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
    public async Task Deserialize_WithUnresolvedReusableActionReference_ShouldAddDiagnosticError()
    {
        // Arrange
        var json = """
        {
            "overlay": "1.1.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "actions": [
                {
                    "x-$ref": "#/components/actions/missingAction"
                }
            ]
        }
        """;

        // Act
        var (_, diagnostic) = await OverlayDocument.ParseAsync(json);

        // Assert
        Assert.NotNull(diagnostic);
        Assert.Contains(
            diagnostic.Errors,
            static e => e.Pointer == "/actions/0" &&
                        e.Message.Contains("#/components/actions/missingAction", StringComparison.Ordinal));
    }

    [Fact]
#pragma warning disable BOO002
    public async Task Deserialize_WithReusableActionReference_ShouldSetHostDocument()
    {
        // Arrange
        var json = """
        {
            "overlay": "1.1.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "x-components": {
                "actions": {
                    "errorResponse": {
                        "target": "$.paths['/pets'].get.responses.404",
                        "remove": true
                    }
                }
            },
            "actions": [
                {
                    "x-$ref": "#/components/actions/errorResponse"
                }
            ]
        }
        """;

        // Act
        var (overlayDocument, _) = await OverlayDocument.ParseAsync(json);

        // Assert
        Assert.NotNull(overlayDocument);
        Assert.NotNull(overlayDocument.Actions);
        var reference = Assert.IsType<OverlayReusableActionReference>(Assert.Single(overlayDocument.Actions));
        Assert.Same(overlayDocument, reference.Reference.HostDocument);
    }

    [Fact]
#pragma warning disable BOO002
    public async Task ParseAndApply_WithReusableActionReference_ShouldResolveTargetActionFromHostDocument()
    {
        // Arrange
        var overlayJson = """
        {
            "overlay": "1.1.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "x-components": {
                "actions": {
                    "removeNotFoundDescription": {
                        "target": "$.paths['/pets'].get.responses['404'].description",
                        "remove": true
                    }
                }
            },
            "actions": [
                {
                    "x-$ref": "#/components/actions/removeNotFoundDescription"
                }
            ]
        }
        """;
        var targetDocument = JsonNode.Parse("""
        {
            "openapi": "3.1.0",
            "paths": {
                "/pets": {
                    "get": {
                        "responses": {
                            "404": {
                                "description": "Not found"
                            }
                        }
                    }
                }
            }
        }
        """)!;

        // Act
        var (overlayDocument, parseDiagnostic) = await OverlayDocument.ParseAsync(overlayJson);

        // Assert parse output
        Assert.NotNull(overlayDocument);
        Assert.NotNull(parseDiagnostic);
        Assert.NotNull(overlayDocument.Actions);
        var reference = Assert.IsType<OverlayReusableActionReference>(Assert.Single(overlayDocument.Actions));
        Assert.Same(overlayDocument, reference.Reference.HostDocument);
        Assert.Empty(parseDiagnostic.Errors);

        // Act
        var applyDiagnostic = new OverlayDiagnostic();
        var result = overlayDocument.ApplyToDocument(targetDocument, applyDiagnostic);

        // Assert apply output
        Assert.True(result);
        Assert.Empty(applyDiagnostic.Errors);
        Assert.Empty(applyDiagnostic.Warnings);
        Assert.Null(targetDocument["paths"]?["/pets"]?["get"]?["responses"]?["404"]?["description"]);
    }
#pragma warning restore BOO002

    [Fact]
#pragma warning disable BOO002
    public void SerializeAsV1_1_WithUnresolvedReusableActionReference_ShouldThrow()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Actions =
            [
                new OverlayReusableActionReference
                {
                    Reference = new OverlayReusableActionReferenceItem
                    {
                        Id = "missingAction"
                    }
                }
            ]
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Act + Assert
        var exception = Assert.Throws<InvalidOperationException>(() => overlayDocument.SerializeAsV1_1(writer));
        Assert.Contains("/actions/0", exception.Message, StringComparison.Ordinal);
        Assert.Contains("#/components/actions/missingAction", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
#pragma warning disable BOO002
    public void SerializeAsV1_1_WithReusableActionReferenceWithoutHostDocument_ShouldSetHostDocument()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Components = new OverlayComponents
            {
                Actions = new Dictionary<string, OverlayReusableAction>
                {
                    ["errorResponse"] = new()
                    {
                        Target = "$.paths['/pets'].get.responses.404",
                        Remove = true
                    }
                }
            },
            Actions =
            [
                new OverlayReusableActionReference
                {
                    Reference = new OverlayReusableActionReferenceItem
                    {
                        Id = "errorResponse"
                    }
                }
            ]
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Act
        overlayDocument.SerializeAsV1_1(writer);

        // Assert
        var reference = Assert.IsType<OverlayReusableActionReference>(Assert.Single(overlayDocument.Actions));
        Assert.Same(overlayDocument, reference.Reference.HostDocument);
    }
#pragma warning restore BOO002
#pragma warning restore BOO002

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
            "overlay": "1.1.0",
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
        var (overlayDocument, _) = await OverlayDocument.LoadFromStreamAsync(stream, cancellationToken: TestContext.Current.CancellationToken);

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

        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".json");
        await File.WriteAllTextAsync(tempFile, json, TestContext.Current.CancellationToken);

        // Act
        var (overlayDocument, _) = await OverlayDocument.LoadFromUrlAsync(tempFile, token: TestContext.Current.CancellationToken);

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

    [Fact]
#pragma warning disable BOO002
    public void CombineWith_MergesComponents()
    {
        // Given
        var overlayDocument1 = new OverlayDocument
        {
            Components = new OverlayComponents
            {
                Actions = new Dictionary<string, OverlayReusableAction>
                {
                    { "setTitle", new OverlayReusableAction { Target = "$.info.title", Update = JsonNode.Parse("\"A\"") } },
                    { "setVersion", new OverlayReusableAction { Target = "$.info.version", Update = JsonNode.Parse("\"1.0.0\"") } }
                }
            }
        };
        var overlayDocument2 = new OverlayDocument
        {
            Components = new OverlayComponents
            {
                Actions = new Dictionary<string, OverlayReusableAction>
                {
                    { "setVersion", new OverlayReusableAction { Target = "$.info.version", Update = JsonNode.Parse("\"2.0.0\"") } },
                    { "setDescription", new OverlayReusableAction { Target = "$.info.description", Update = JsonNode.Parse("\"desc\"") } }
                }
            }
        };

        // When
        var result = overlayDocument1.CombineWith(overlayDocument2);

        // Then
        Assert.NotNull(result.Components);
        Assert.NotNull(result.Components.Actions);
        Assert.Equal(3, result.Components.Actions.Count);
        Assert.Equal("$.info.title", result.Components.Actions["setTitle"].Target);
        Assert.Equal("2.0.0", result.Components.Actions["setVersion"].Update?.GetValue<string>());
        Assert.Equal("$.info.description", result.Components.Actions["setDescription"].Target);
    }
#pragma warning restore BOO002

    [Fact]
#pragma warning disable BOO002
    public void Deserialize_WithReusableActionReference_ShouldCreateReferenceAction()
    {
        // Arrange
        var json = """
        {
            "overlay": "1.1.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "actions": [
                {
                    "x-$ref": "#/components/actions/errorResponse",
                    "x-parameterValues": {
                        "region": "us"
                    },
                    "target": "$.paths['/pets'].get.responses"
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
        Assert.NotNull(overlayDocument.Actions);
        var reference = Assert.IsType<OverlayReusableActionReference>(Assert.Single(overlayDocument.Actions));
        Assert.NotNull(reference.Reference);
        Assert.Equal("errorResponse", reference.Reference.Id);
        Assert.Equal("#/components/actions/errorResponse", reference.Reference.Reference);
        Assert.NotNull(reference.Reference.ParameterValues);
        Assert.Equal("us", reference.Reference.ParameterValues["region"]);
        Assert.Equal("$.paths['/pets'].get.responses", reference.Target);
    }
#pragma warning restore BOO002
}