using System.Text.Json.Nodes;

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
            "overlay": "1.2.0",
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
        overlayDocument.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
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
            Extends = "base.yaml#fragment"
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Then
        var exception = Assert.Throws<InvalidOperationException>(() => overlayDocument.SerializeAsV1_2(writer));
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
                        Id = "errorResponse"
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
            "overlay": "1.2.0",
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
        overlayDocument.SerializeAsV1_2(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }
}