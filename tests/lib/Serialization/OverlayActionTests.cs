using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

#pragma warning disable BOO001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayActionTests
{
    [Fact]
    public void SerializeAsV1_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayAction = new OverlayAction
        {
            Target = "Test Target",
            Description = "Test Description",
            Remove = true
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "target": "Test Target",
    "description": "Test Description",
    "remove": true
}
""";

        // Act
        overlayAction.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);


        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_WithUpdate_ShouldWriteCorrectJson()
    {
        // Arrange
        var updateNode = JsonNode.Parse("""
        {
            "summary": "Updated summary",
            "description": "Updated description",
            "operationId": "updateOperation"
        }
        """);

        var overlayAction = new OverlayAction
        {
            Target = "Test Target",
            Description = "Test Description",
            Remove = false,
            Update = updateNode
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "target": "Test Target",
    "description": "Test Description",
    "update": {
        "summary": "Updated summary",
        "description": "Updated description",
        "operationId": "updateOperation"
    }
}
""";

        // Act
        overlayAction.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_WithUpdateArray_ShouldWriteCorrectJson()
    {
        // Arrange
        var updateNode = JsonNode.Parse("""
        ["tag1", "tag2", "tag3"]
        """);

        var overlayAction = new OverlayAction
        {
            Target = "Test Target",
            Description = "Test Description",
            Update = updateNode
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "target": "Test Target",
    "description": "Test Description",
    "update": ["tag1", "tag2", "tag3"]
}
""";

        // Act
        overlayAction.SerializeAsV1(writer);
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
            "remove": true
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);


        // Act
        var overlayAction = OverlayV1Deserializer.LoadAction(parseNode);

        // Assert
        Assert.Equal("Test Target", overlayAction.Target);
        Assert.Equal("Test Description", overlayAction.Description);
        Assert.True(overlayAction.Remove);
    }

    [Fact]
    public void Deserialize_WithUpdate_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "target": "Test Target",
            "description": "Test Description",
            "remove": false,
            "update": {
                "summary": "Updated summary",
                "description": "Updated description",
                "operationId": "updateOperation"
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayAction = OverlayV1Deserializer.LoadAction(parseNode);

        // Assert
        Assert.Equal("Test Target", overlayAction.Target);
        Assert.Equal("Test Description", overlayAction.Description);
        Assert.False(overlayAction.Remove);
        Assert.NotNull(overlayAction.Update);

        var updateObject = overlayAction.Update.AsObject();
        Assert.Equal("Updated summary", updateObject["summary"]?.GetValue<string>());
        Assert.Equal("Updated description", updateObject["description"]?.GetValue<string>());
        Assert.Equal("updateOperation", updateObject["operationId"]?.GetValue<string>());
    }

    [Fact]
    public void Deserialize_WithUpdateArray_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "target": "Test Target",
            "description": "Test Description",
            "update": ["tag1", "tag2", "tag3"]
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayAction = OverlayV1Deserializer.LoadAction(parseNode);

        // Assert
        Assert.Equal("Test Target", overlayAction.Target);
        Assert.Equal("Test Description", overlayAction.Description);
        Assert.NotNull(overlayAction.Update);

        var updateArray = overlayAction.Update.AsArray();
        Assert.Equal(3, updateArray.Count);
        Assert.Equal("tag1", updateArray[0]?.GetValue<string>());
        Assert.Equal("tag2", updateArray[1]?.GetValue<string>());
        Assert.Equal("tag3", updateArray[2]?.GetValue<string>());
    }

    [Fact]
    public void Deserialize_WithUpdatePrimitiveValue_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "target": "Test Target",
            "description": "Test Description",
            "update": "simple string value"
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var overlayAction = OverlayV1Deserializer.LoadAction(parseNode);

        // Assert
        Assert.Equal("Test Target", overlayAction.Target);
        Assert.Equal("Test Description", overlayAction.Description);
        Assert.NotNull(overlayAction.Update);
        Assert.Equal("simple string value", overlayAction.Update.GetValue<string>());
    }
    [Fact]
    public void ApplyToDocument_ShouldFailNoNullJsonNode()
    {
        var overlayAction = new OverlayAction
        {
            Target = "Test Target",
            Remove = true
        };
        JsonNode? jsonNode = null;
        var overlayDiagnostic = new OverlayDiagnostic();

        Assert.Throws<ArgumentNullException>(() => overlayAction.ApplyToDocument(jsonNode!, overlayDiagnostic, 0));
    }
    [Fact]
    public void ApplyToDocument_ShouldFailNoDiagnostics()
    {
        var overlayAction = new OverlayAction
        {
            Target = "Test Target",
            Remove = true
        };
        var jsonNode = JsonNode.Parse("{}")!;
        OverlayDiagnostic? overlayDiagnostic = null;

        Assert.Throws<ArgumentNullException>(() => overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic!, 0));
    }
    [Fact]
    public void ApplyToDocument_ShouldFailNoTarget()
    {
        var overlayAction = new OverlayAction
        {
            Remove = true
        };
        var jsonNode = JsonNode.Parse("{}")!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.False(result);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Equal("$.actions[0]", overlayDiagnostic.Errors[0].Pointer);
        Assert.Equal("Target is required", overlayDiagnostic.Errors[0].Message);
    }
    [Fact]
    public void ApplyToDocument_ShouldFailNoRemoveOrUpdate()
    {
        var overlayAction = new OverlayAction
        {
            Target = "Test Target"
        };
        var jsonNode = JsonNode.Parse("{}")!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.False(result);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Equal("$.actions[0]", overlayDiagnostic.Errors[0].Pointer);
        Assert.Equal("At least one of 'remove', 'update' or 'x-copy' must be specified", overlayDiagnostic.Errors[0].Message);
    }
    [Fact]
    public void ApplyToDocument_ShouldFailBothRemoveAndUpdate()
    {
        var overlayAction = new OverlayAction
        {
            Target = "Test Target",
            Remove = true,
            Update = JsonNode.Parse("{}")
        };
        var jsonNode = JsonNode.Parse("{}")!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.False(result);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Equal("$.actions[0]", overlayDiagnostic.Errors[0].Pointer);
        Assert.Equal("At most one of 'remove', 'update' or 'x-copy' can be specified", overlayDiagnostic.Errors[0].Message);
    }
    [Fact]
    public void ApplyToDocument_ShouldFailInvalidJsonPath()
    {
        var overlayAction = new OverlayAction
        {
            Target = "Test Target",
            Remove = true
        };
        var jsonNode = JsonNode.Parse("{}")!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.False(result);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Equal("$.actions[0]", overlayDiagnostic.Errors[0].Pointer);
        Assert.Equal("Invalid JSON Path: 'Test Target'", overlayDiagnostic.Errors[0].Message);
    }
    [Fact]
    public void ApplyToDocument_ShouldRemoveANode()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.info.title",
            Remove = true
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Test API",
                "version": "1.0.0"
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.True(result);
        Assert.Null(jsonNode["info"]?["title"]);
        Assert.Empty(overlayDiagnostic.Errors);
    }
    [Fact]
    public void ApplyToDocument_ShouldRemoveANodeAndNotErrorInWildcard()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.components.schemas['Foo'].anyOf[*].default",
            Remove = true
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Test API",
                "version": "1.0.0"
            },
            "components": {
                "schemas": {
                    "Foo": {
                        "anyOf": [
                            {
                                "default": "value1"
                            },
                            {
                                "type": "string"
                            }
                        ]
                    }
                }
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.True(result);
        Assert.Null(jsonNode["components"]?["schemas"]?["Foo"]?["anyOf"]?[0]?["default"]);
        Assert.Null(jsonNode["components"]?["schemas"]?["Foo"]?["anyOf"]?[1]?["default"]);
        Assert.Equal("string", jsonNode["components"]?["schemas"]?["Foo"]?["anyOf"]?[1]?["type"]?.GetValue<string>());
        Assert.Empty(overlayDiagnostic.Errors);
    }
    [Fact]
    public void ApplyToDocument_ShouldUpdateANode()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.info",
            Update = JsonNode.Parse("""
            {
                "title": "Updated API"
            }
            """)
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Test API",
                "version": "1.0.0"
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.True(result);
        Assert.Equal("Updated API", jsonNode["info"]?["title"]?.GetValue<string>());
        Assert.Empty(overlayDiagnostic.Errors);
    }

    [Fact]
    public void ApplyToDocument_ShouldCopySimpleValue()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.info.title",
            Copy = "$.info.version"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Test API",
                "version": "1.0.0"
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.True(result);
        Assert.Equal("1.0.0", jsonNode["info"]?["title"]?.GetValue<string>());
        Assert.Empty(overlayDiagnostic.Errors);
    }

    [Fact]
    public void ApplyToDocument_ShouldCopyComplexObject()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.paths['/users'].get.responses['200']",
            Copy = "$.components.responses.UserResponse"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "paths": {
                "/users": {
                    "get": {
                        "responses": {
                            "200": {
                                "description": "Old description"
                            }
                        }
                    }
                }
            },
            "components": {
                "responses": {
                    "UserResponse": {
                        "description": "Successful response",
                        "content": {
                            "application/json": {
                                "schema": {
                                    "$ref": "#/components/schemas/User"
                                }
                            }
                        }
                    }
                }
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.True(result);
        Assert.Equal("Successful response", jsonNode["paths"]?["/users"]?["get"]?["responses"]?["200"]?["description"]?.GetValue<string>());
        Assert.NotNull(jsonNode["paths"]?["/users"]?["get"]?["responses"]?["200"]?["content"]);
        Assert.Empty(overlayDiagnostic.Errors);
    }

    [Fact]
    public void ApplyToDocument_ShouldCopyArrayElements()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.paths['/users'].get.tags",
            Copy = "$.paths['/posts'].get.tags"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "paths": {
                "/users": {
                    "get": {
                        "tags": ["user"]
                    }
                },
                "/posts": {
                    "get": {
                        "tags": ["post", "content"]
                    }
                }
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.True(result);
        var targetTags = jsonNode["paths"]?["/users"]?["get"]?["tags"]?.AsArray();
        Assert.NotNull(targetTags);
        Assert.Equal(2, targetTags.Count);
        Assert.Equal("post", targetTags[0]?.GetValue<string>());
        Assert.Equal("content", targetTags[1]?.GetValue<string>());
        Assert.Empty(overlayDiagnostic.Errors);
    }

    [Fact]
    public void ApplyToDocument_ShouldCopyMultipleMatches()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.paths[*].get.summary",
            Copy = "$.paths[*].get.operationId"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "paths": {
                "/users": {
                    "get": {
                        "summary": "Get users",
                        "operationId": "listUsers"
                    }
                },
                "/posts": {
                    "get": {
                        "summary": "Get posts",
                        "operationId": "listPosts"
                    }
                }
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.True(result);
        Assert.Equal("listUsers", jsonNode["paths"]?["/users"]?["get"]?["summary"]?.GetValue<string>());
        Assert.Equal("listPosts", jsonNode["paths"]?["/posts"]?["get"]?["summary"]?.GetValue<string>());
        Assert.Empty(overlayDiagnostic.Errors);
    }

    [Fact]
    public void ApplyToDocument_CopyShouldFailWithInvalidCopyPath()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.info.title",
            Copy = "invalid path"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Test API"
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.False(result);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Equal("$.actions[0]", overlayDiagnostic.Errors[0].Pointer);
        Assert.Equal("Invalid copy JSON Path: 'invalid path'", overlayDiagnostic.Errors[0].Message);
    }

    [Fact]
    public void ApplyToDocument_CopyShouldFailWhenCopyTargetNotFound()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.info.title",
            Copy = "$.nonexistent.field"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Test API"
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.False(result);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Equal("$.actions[0]", overlayDiagnostic.Errors[0].Pointer);
        Assert.Equal("Copy target '$.nonexistent.field' must point to at least one JSON node", overlayDiagnostic.Errors[0].Message);
    }

    [Fact]
    public void ApplyToDocument_CopyShouldFailWhenCopyTargetHasNoMatches()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.info.title",
            Copy = "$.paths[*].get.nonexistent"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Test API"
            },
            "paths": {
                "/users": {
                    "get": {
                        "summary": "Get users"
                    }
                }
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.False(result);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Equal("$.actions[0]", overlayDiagnostic.Errors[0].Pointer);
        Assert.Equal("Copy target '$.paths[*].get.nonexistent' must point to at least one JSON node", overlayDiagnostic.Errors[0].Message);
    }

    [Fact]
    public void ApplyToDocument_CopyShouldFailWhenMatchCountsDiffer()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.paths[*].get.summary",
            Copy = "$.info.title"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Test API"
            },
            "paths": {
                "/users": {
                    "get": {
                        "summary": "Get users"
                    }
                },
                "/posts": {
                    "get": {
                        "summary": "Get posts"
                    }
                }
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.False(result);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Equal("$.actions[0]", overlayDiagnostic.Errors[0].Pointer);
        Assert.Equal("The number of matches for 'target' (2) and 'x-copy' (1) must be the same", overlayDiagnostic.Errors[0].Message);
    }

    [Fact]
    public void ApplyToDocument_CopyShouldFailWhenTargetPointsToNull()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.info.nonexistent",
            Copy = "$.info.title"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Test API",
                "nonexistent": null
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.False(result);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Equal("$.actions[0]", overlayDiagnostic.Errors[0].Pointer);
        Assert.Equal("Target '$.info.nonexistent' does not point to a valid JSON node", overlayDiagnostic.Errors[0].Message);
    }

    [Fact]
    public void ApplyToDocument_CopyShouldFailWhenCopyTargetPointsToNull()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.info.title",
            Copy = "$.info.nonexistent"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Test API",
                "nonexistent": null
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.False(result);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Equal("$.actions[0]", overlayDiagnostic.Errors[0].Pointer);
        Assert.Equal("Copy target '$.info.nonexistent' does not point to a valid JSON node", overlayDiagnostic.Errors[0].Message);
    }

    [Fact]
    public void ApplyToDocument_CopyShouldMergeObjectsCorrectly()
    {
        var overlayAction = new OverlayAction
        {
            Target = "$.info",
            Copy = "$.components.info"
        };
        var jsonNode = JsonNode.Parse("""
        {
            "info": {
                "title": "Original API",
                "version": "1.0.0"
            },
            "components": {
                "info": {
                    "title": "Updated API",
                    "description": "New description"
                }
            }
        }
        """)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        var result = overlayAction.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        Assert.True(result);
        Assert.Equal("Updated API", jsonNode["info"]?["title"]?.GetValue<string>());
        Assert.Equal("1.0.0", jsonNode["info"]?["version"]?.GetValue<string>());
        Assert.Equal("New description", jsonNode["info"]?["description"]?.GetValue<string>());
        Assert.Empty(overlayDiagnostic.Errors);
    }

    [Fact]
    public void SerializeAsV1_WithCopy_ShouldWriteCorrectJson()
    {
        // Arrange
        var overlayAction = new OverlayAction
        {
            Target = "$.info.title",
            Description = "Copy description to title",
            Copy = "$.info.description"
        };
        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "target": "$.info.title",
    "description": "Copy description to title",
    "x-copy": "$.info.description"
}
""";

        // Act
        overlayAction.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void ApplyToDocument_WorksWithDoubleDotsExpressions()
    {
        // Given
        var action = new OverlayAction
        {
            Target = "$..[?(@['$ref'] == '#/components/schemas/Foo')]",
            Update = JsonNode.Parse(
"""
{
    "anyOf":
    [
        {
            "$ref": "#/components/schemas/Foo"
        },
        {
            "type": "null"
        }
    ]
}
"""
            )
        };

        var documentJson =
"""
{
    "openapi": "3.0.0",
    "info": {
        "title": "(title)",
        "version": "0.0.0"
    },
    "tags": [],
    "paths": {},
    "components": {
        "schemas": {
            "Bar": {
                "type": "object",
                "required": [
                    "foo"
                ],
                "properties": {
                    "foo": {
                        "$ref": "#/components/schemas/Foo"
                    }
                }
            },
            "Baz": {
                "type": "object",
                "required": [
                    "foo"
                ],
                "properties": {
                    "foo": {
                        "$ref": "#/components/schemas/Foo"
                    }
                }
            },
            "Foo": {
                "type": "object"
            }
        }
    }
}
""";
        var jsonNode = JsonNode.Parse(documentJson)!;
        var overlayDiagnostic = new OverlayDiagnostic();

        // When
        var result = action.ApplyToDocument(jsonNode, overlayDiagnostic, 0);

        // Then
        Assert.True(result);
        Assert.Empty(overlayDiagnostic.Errors);
        var barFoo = Assert.IsType<JsonObject>(jsonNode["components"]?["schemas"]?["Bar"]?["properties"]?["foo"]);
        var bazFoo = Assert.IsType<JsonObject>(jsonNode["components"]?["schemas"]?["Baz"]?["properties"]?["foo"]);
        var barAnyOf = Assert.IsType<JsonArray>(barFoo["anyOf"]);
        var bazAnyOf = Assert.IsType<JsonArray>(bazFoo["anyOf"]);
        Assert.Equal(2, barAnyOf.Count);
        Assert.Equal(2, bazAnyOf.Count);
        Assert.True(JsonNode.DeepEquals(barAnyOf, bazAnyOf));
    }
}