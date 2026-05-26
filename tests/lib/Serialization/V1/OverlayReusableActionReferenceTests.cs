using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

#pragma warning disable BOO002
public class OverlayReusableActionReferenceV1Tests
{
    [Fact]
    public void SerializeAsV1_ShouldWriteOnlySetActionOverridesAndReferenceFields()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                Target = "$.paths['/pets'].get.responses",
                Description = "Override Description",
                Remove = false,
                Update = JsonNode.Parse("""
                {
                    "summary": "Updated summary"
                }
                """)
            }
        };

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "x-$ref": "#/components/actions/errorResponse",
    "target": "$.paths['/pets'].get.responses",
    "description": "Override Description",
    "update": {
        "summary": "Updated summary"
    }
}
""";

        // Act
        reference.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_ShouldIgnoreInheritedFieldsFromTargetAction()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse"
            },
            TargetAction = new OverlayReusableAction
            {
                Description = "Reusable description",
                Fields = new OverlayAction
                {
                    Description = "from target action",
                    Remove = true,
                    Update = JsonNode.Parse("""{ "title": "x" }""")
                }
            }
        };

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "x-$ref": "#/components/actions/errorResponse"
}
""";

        // Act
        reference.SerializeAsV1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SerializeAsV1_WithMissingReference_ShouldThrow(string? missingReference)
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = missingReference
            }
        };

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        // Act + Assert
        var exception = Assert.Throws<InvalidOperationException>(() => reference.SerializeAsV1(writer));
        Assert.Contains("cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Getters_ShouldFallbackToTargetActionFields_WhenBackingFieldNotSet()
    {
        // Arrange
        var targetAction = new OverlayReusableAction
        {
            Description = "Reusable description",
            Fields = new OverlayAction
            {
                Description = "Target Description",
                Remove = true,
                Update = JsonNode.Parse("""{ "x": 1 }"""),
                Copy = "$.other"
            }
        };
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                Target = "$.paths"
            },
            TargetAction = targetAction
        };

        // Assert
        Assert.NotNull(reference.Reference);
        Assert.Equal("#/components/actions/errorResponse", reference.Reference.Reference);
        Assert.Equal("$.paths", reference.Target);
        Assert.Equal("Target Description", reference.Description);
        Assert.True(reference.Remove);
        Assert.Equal(1, reference.Update?["x"]?.GetValue<int>());
        Assert.Equal("$.other", reference.Copy);
    }

    [Fact]
    public void Getters_ShouldPreferBackingFieldsOverTargetAction()
    {
        // Arrange
        var targetAction = new OverlayReusableAction
        {
            Fields = new OverlayAction
            {
                Description = "Target Description",
                Remove = false,
                Update = JsonNode.Parse("""{ "x": 1 }"""),
                Copy = "$.other"
            }
        };
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                Target = "$.overridden",
                Description = "Local Description",
                Remove = true,
                Update = JsonNode.Parse("""{ "x": 2 }"""),
                Copy = "$.localCopy"
            },
            TargetAction = targetAction,
        };

        // Assert
        Assert.Equal("$.overridden", reference.Target);
        Assert.Equal("Local Description", reference.Description);
        Assert.True(reference.Remove);
        Assert.Equal(2, reference.Update?["x"]?.GetValue<int>());
        Assert.Equal("$.localCopy", reference.Copy);
    }

    [Fact]
    public void ConstructorWithHostDocument_ShouldResolveTargetActionAndApplyFallbackRules()
    {
        // Arrange
        var resolvedAction = new OverlayReusableAction
        {
            Description = "Reusable description",
            Fields = new OverlayAction
            {
                Description = "Resolved reusable action",
                Remove = false,
                Update = JsonNode.Parse("""
                {
                    "404": {
                        "description": "Not found"
                    }
                }
                """),
                Copy = "$.paths['/pets'].post.responses"
            }
        };

        var hostDocument = new OverlayDocument
        {
            Components = new OverlayComponents
            {
                Actions = new Dictionary<string, OverlayReusableAction>
                {
                    ["errorResponse"] = resolvedAction
                }
            }
        };

        var reference = new OverlayReusableActionReference("errorResponse", hostDocument)
        {
            Reference = new OverlayReusableActionReferenceItem("errorResponse", hostDocument)
            {
                Target = "$.paths['/pets'].get.responses"
            },
            Remove = true
        };

        // Assert
        Assert.Equal("#/components/actions/errorResponse", reference.Reference.Reference);
        Assert.Same(resolvedAction, reference.TargetAction);
        Assert.Equal("$.paths['/pets'].get.responses", reference.Target);
        Assert.Equal("Resolved reusable action", reference.Description);
        Assert.True(reference.Remove);
        Assert.Equal("Not found", reference.Update?["404"]?["description"]?.GetValue<string>());
        Assert.Equal("$.paths['/pets'].post.responses", reference.Copy);
    }

    [Fact]
    public void ConstructorWithHostDocument_WithCanonicalReferencePointer_ShouldResolveTargetAction()
    {
        // Arrange
        var resolvedAction = new OverlayReusableAction
        {
            Description = "Resolved reusable action"
        };

        var hostDocument = new OverlayDocument
        {
            Components = new OverlayComponents
            {
                Actions = new Dictionary<string, OverlayReusableAction>
                {
                    ["errorResponse"] = resolvedAction
                }
            }
        };

        // Act
        var reference = new OverlayReusableActionReference("#/components/actions/errorResponse", hostDocument);

        // Assert
        Assert.Equal("errorResponse", reference.Reference.Id);
        Assert.Equal("#/components/actions/errorResponse", reference.Reference.Reference);
        Assert.Same(resolvedAction, reference.TargetAction);
    }

    [Fact]
    public void GetResolvedAction_WithAllValuesResolved_ShouldReturnOverlayActionAndNoDiagnostics()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                Target = "$.paths['/pets'].get.responses",
                Description = "Resolved reusable action",
                Remove = false,
                Update = JsonNode.Parse("""{ "x-region": "overridden" }"""),
                Copy = "$.paths['/pets'].post.responses"
            },
            TargetAction = new OverlayReusableAction()
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var resolvedAction = reference.GetResolvedAction(overlayDiagnostic);

        // Assert
        Assert.NotNull(resolvedAction);
        Assert.Equal("$.paths['/pets'].get.responses", resolvedAction!.Target);
        Assert.Equal("Resolved reusable action", resolvedAction.Description);
        Assert.False(resolvedAction.Remove);
        Assert.Equal("overridden", resolvedAction.Update?["x-region"]?.GetValue<string>());
        Assert.Equal("$.paths['/pets'].post.responses", resolvedAction.Copy);
        Assert.Empty(overlayDiagnostic.Errors);
    }

    [Fact]
    public void GetResolvedAction_WithoutTarget_ShouldAddDiagnosticAndReturnNull()
    {
        // Arrange
        var hostDocument = new OverlayDocument
        {
            Components = new OverlayComponents
            {
                Actions = new Dictionary<string, OverlayReusableAction>
                {
                    ["errorResponse"] = new OverlayReusableAction
                    {
                        Fields = new OverlayAction { Description = "Adds an error response" }
                    }
                }
            }
        };
        var reference = new OverlayReusableActionReference("errorResponse", hostDocument);
        var diagnostic = new OverlayDiagnostic();

        // Act
        var resolved = reference.GetResolvedAction(diagnostic);

        // Assert
        Assert.Null(resolved);
        Assert.Single(diagnostic.Errors);
        Assert.Contains("target", diagnostic.Errors[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetResolvedAction_WithUnresolvedTargetAction_ShouldAddDiagnosticAndReturnNull()
    {
        // Arrange
        var hostDocument = new OverlayDocument();
        var reference = new OverlayReusableActionReference("errorResponse", hostDocument);
        var diagnostic = new OverlayDiagnostic();

        // Act
        var resolved = reference.GetResolvedAction(diagnostic);

        // Assert
        Assert.Null(resolved);
        Assert.Single(diagnostic.Errors);
        Assert.Contains("could not be resolved", diagnostic.Errors[0].Message);
    }

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "x-$ref": "#/components/actions/errorResponse",
            "target": "$.paths['/pets'].get.responses",
            "description": "Override Description",
            "remove": false,
            "x-copy": "$.paths['/pets'].post.responses",
            "update": {
                "404": {
                    "description": "Not found"
                }
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());

        // Act
        var reference = OverlayV1Deserializer.LoadReusableActionReference(jsonNode, parsingContext);

        // Assert
        Assert.NotNull(reference.Reference);
        Assert.Equal("errorResponse", reference.Reference.Id);
        Assert.Equal("#/components/actions/errorResponse", reference.Reference.Reference);
        Assert.Equal("$.paths['/pets'].get.responses", reference.Target);
        Assert.Equal("Override Description", reference.Description);
        Assert.False(reference.Remove);
        Assert.Equal("$.paths['/pets'].post.responses", reference.Copy);
        Assert.Equal("Not found", reference.Update?["404"]?["description"]?.GetValue<string>());
    }
}
#pragma warning restore BOO002