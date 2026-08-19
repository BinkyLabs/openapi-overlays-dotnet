using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_2;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public sealed class OverlayDocumentV1_2Tests
{
    [Fact]
    public void SerializeAsV1_2_WithComponents_ShouldWriteCorrectJson()
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
                            Fields = new OverlayAction
                            {
                                Update = JsonNode.Parse("""
                                {
                                    "url": "https://api.example.com"
                                }
                                """)
                            }
                        }
                    }
                }
            }
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson = """
        {
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "components": {
                "actions": {
                    "setServerUrl": {
                        "fields": {
                            "update": {
                                "url": "https://api.example.com"
                            }
                        }
                    }
                }
            }
        }
        """;

        // Act
        overlayDocument.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void Deserialize_WithComponents_ShouldSetPropertiesCorrectly()
    {
        var json = """
        {
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "2.0.0"
            },
            "components": {
                "actions": {
                    "setServerUrl": {
                        "fields": {
                            "target": "$.servers[0]",
                            "copy": "$.servers[1]"
                        },
                        "description": "Sets the server URL"
                    }
                }
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var overlayDocument = OverlayV1_2Deserializer.LoadDocument(jsonNode, parsingContext);

        Assert.NotNull(overlayDocument.Components);
        Assert.NotNull(overlayDocument.Components.Actions);
        var action = Assert.Single(overlayDocument.Components.Actions);
        Assert.Equal("setServerUrl", action.Key);
        Assert.Equal("Sets the server URL", action.Value.Description);
        Assert.NotNull(action.Value.Fields);
        Assert.Equal("$.servers[0]", action.Value.Fields.Target);
        Assert.Equal("$.servers[1]", action.Value.Fields.Copy);
    }

    [Fact]
    public void SerializeAsV1_2_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Extends = new("./x-extends", UriKind.RelativeOrAbsolute),
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
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "extends": "./x-extends",
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
        overlayDocument.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        var json = """
        {
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "2.0.0"
            },
            "extends": "./x-extends",
            "$self": "https://example.com/overlays/test",
            "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "remove": true
                },
                {
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

        var overlayDocument = OverlayV1_2Deserializer.LoadDocument(jsonNode, parsingContext);

        Assert.Equal("1.2.0", overlayDocument.Overlay);
        Assert.Equal("Test Overlay", overlayDocument.Info?.Title);
        Assert.Equal("2.0.0", overlayDocument.Info?.Version);
        Assert.Equal(new("./x-extends", UriKind.RelativeOrAbsolute), overlayDocument.Extends);
        Assert.Equal(new("https://example.com/overlays/test"), overlayDocument.Self);
        Assert.NotNull(overlayDocument.Extensions);
        var extensionNodeValue = Assert.IsType<JsonNodeExtension>(overlayDocument.Extensions["x-custom-extension"]);
        Assert.Equal("someValue", extensionNodeValue.Node["someProperty"]?.GetValue<string>());
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
    public void SerializeAsV1_2_WithSelf_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Self = new("https://example.com/overlays/test"),
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            }
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson = """
        {
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "$self": "https://example.com/overlays/test"
        }
        """;

        // Act
        overlayDocument.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public async Task ParseAsync_WithV1_2_ShouldReturnDocument()
    {
        var json = """
        {
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "$self": "https://example.com/overlays/test"
        }
        """;

        var (overlayDocument, diagnostic) = await OverlayDocument.ParseAsync(json, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(overlayDocument);
        Assert.NotNull(diagnostic);
        Assert.Equal(OverlaySpecVersion.Overlay1_2, diagnostic.SpecificationVersion);
        Assert.Equal("1.2.0", overlayDocument.Overlay);
        Assert.Equal(new("https://example.com/overlays/test"), overlayDocument.Self);
        Assert.Empty(diagnostic.Errors);
    }

    [Fact]
    public void ExtendsShouldNotContainFragments()
    {
        // Given
        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Extends = new("https://foo.bar/base.yaml#fragment", UriKind.RelativeOrAbsolute)
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Then
        var exception = Assert.Throws<InvalidOperationException>(() => overlayDocument.SerializeAsV1_2(writer));
        Assert.Contains("'extends' property must not contain a fragment identifier", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SelfShouldNotContainFragments()
    {
        // Given
        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Self = new("https://foo.bar/overlay.yaml#fragment", UriKind.RelativeOrAbsolute)
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Then
        var exception = Assert.Throws<InvalidOperationException>(() => overlayDocument.SerializeAsV1_2(writer));
        Assert.Contains("'$self' property must not contain a fragment identifier", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Deserialize_WithDocumentUriFragments_ShouldReportErrors()
    {
        var json = """
        {
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "2.0.0"
            },
            "extends": "https://example.com/openapi.yaml#fragment",
            "$self": "https://example.com/overlays/test.yaml#fragment",
            "actions": [
                {
                    "target": "Test Target",
                    "description": "Test Description",
                    "remove": true
                }
            ]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var overlayDocument = OverlayV1_2Deserializer.LoadDocument(jsonNode, parsingContext);

        Assert.NotNull(overlayDocument.Extends);
        Assert.NotNull(overlayDocument.Self);
        Assert.Equal(2, parsingContext.Diagnostic.Errors.Count);
        Assert.Contains(parsingContext.Diagnostic.Errors, static error => error.Pointer == "#/extends" && error.Message.Contains("must not contain a fragment identifier", StringComparison.Ordinal));
        Assert.Contains(parsingContext.Diagnostic.Errors, static error => error.Pointer == "#/$self" && error.Message.Contains("must not contain a fragment identifier", StringComparison.Ordinal));
    }

    [Fact]
    public void SerializeAsV1_2_WithUnresolvedReusableActionReference_ShouldThrow()
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
        var exception = Assert.Throws<InvalidOperationException>(() => overlayDocument.SerializeAsV1_2(writer));
        Assert.Contains("/actions/0", exception.Message, StringComparison.Ordinal);
        Assert.Contains("#/components/actions/missingAction", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeAsV1_2_WithReusableActionReferenceWithoutHostDocument_ShouldSetHostDocument()
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
                        Fields = new OverlayAction
                        {
                            Remove = true
                        }
                    }
                }
            },
            Actions =
            [
                new OverlayReusableActionReference
                {
                    Reference = new OverlayReusableActionReferenceItem
                    {
                        Id = "errorResponse",
                        Target = "$.paths['/pets'].get.responses"
                    }
                }
            ]
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Act
        overlayDocument.SerializeAsV1_2(writer);

        // Assert
        var reference = Assert.IsType<OverlayReusableActionReference>(Assert.Single(overlayDocument.Actions));
        Assert.Same(overlayDocument, reference.Reference.HostDocument);
    }

    [Fact]
    public async Task Deserialize_WithUnresolvedReusableActionReference_ShouldAddDiagnosticError()
    {
        var json = """
        {
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "actions": [
                {
                    "$ref": "#/components/actions/missingAction"
                }
            ]
        }
        """;

        var (_, diagnostic) = await OverlayDocument.ParseAsync(json, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(diagnostic);
        Assert.Contains(
            diagnostic.Errors,
            static e => e.Pointer == "/actions/0" &&
                        e.Message.Contains("#/components/actions/missingAction", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Deserialize_WithReusableActionReference_ShouldSetHostDocument()
    {
        var json = """
        {
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "components": {
                "actions": {
                    "errorResponse": {
                        "fields": {
                            "remove": true
                        }
                    }
                }
            },
            "actions": [
                {
                    "$ref": "#/components/actions/errorResponse"
                }
            ]
        }
        """;

        var (overlayDocument, diagnostic) = await OverlayDocument.ParseAsync(json, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(diagnostic);
        Assert.Empty(diagnostic.Errors);
        Assert.NotNull(overlayDocument);
        Assert.NotNull(overlayDocument.Actions);
        var reference = Assert.IsType<OverlayReusableActionReference>(Assert.Single(overlayDocument.Actions));
        Assert.Same(overlayDocument, reference.Reference.HostDocument);
    }

    [Fact]
    public void Deserialize_WithReusableActionReference_ShouldCreateReferenceAction()
    {
        var json = """
        {
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "actions": [
                {
                    "$ref": "#/components/actions/errorResponse",
                    "target": "$.paths['/pets'].get.responses"
                }
            ]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        var overlayDocument = OverlayV1_2Deserializer.LoadDocument(jsonNode, parsingContext);

        Assert.NotNull(overlayDocument.Actions);
        var reference = Assert.IsType<OverlayReusableActionReference>(Assert.Single(overlayDocument.Actions));
        Assert.Equal("errorResponse", reference.Reference.Id);
        Assert.Equal("#/components/actions/errorResponse", reference.Reference.Reference);
        Assert.Equal("$.paths['/pets'].get.responses", reference.Target);
    }

    [Fact]
    public void SerializeAsV1_2_WithUpdate_ShouldWriteCorrectJson()
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
            Extends = new("./x-extends", UriKind.RelativeOrAbsolute),
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
            "overlay": "1.2.0",
            "info": {
                "title": "Test Overlay",
                "version": "1.0.0"
            },
            "extends": "./x-extends",
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
        overlayDocument.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }
}