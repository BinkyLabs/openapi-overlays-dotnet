using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

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
        Assert.Equal("Either 'remove' or 'update' must be specified", overlayDiagnostic.Errors[0].Message);
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
        Assert.Equal("'remove' and 'update' cannot be used together", overlayDiagnostic.Errors[0].Message);
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
}