using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

using Microsoft.OpenApi;

using ParsingContext = BinkyLabs.OpenApi.Overlays.Reader.ParsingContext;

namespace BinkyLabs.OpenApi.Overlays.Tests;

#pragma warning disable BOO002
public class OverlayReusableActionReferenceV1_1Tests
{
    [Fact]
    public void SerializeAsV1_1_ShouldWriteOnlySetActionOverridesAndReferenceFields()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                ParameterValues = new Dictionary<string, string>
                {
                    { "region", "us" }
                }
            },
            Description = "Override Description",
            Remove = false,
            Copy = "$.paths['/pets'].post.responses",
            Update = JsonNode.Parse("""
            {
                "summary": "Updated summary"
            }
            """)
        };

        using var textWriter = new StringWriter();
        var writer = new OpenApiJsonWriter(textWriter);

        var expectedJson =
"""
{
    "x-$ref": "#/components/actions/errorResponse",
    "x-parameterValues": {
        "region": "us"
    },
    "description": "Override Description",
    "update": {
        "summary": "Updated summary"
    },
    "copy": "$.paths['/pets'].post.responses"
}
""";

        // Act
        reference.SerializeAsV1_1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Fact]
    public void SerializeAsV1_1_ShouldIgnoreInterfaceFieldsWhenOnlyInheritedFromTargetAction()
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
                Fields = new OverlayAction
                {
                    Target = "$.info",
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
        reference.SerializeAsV1_1(writer);
        var jsonResult = textWriter.ToString();
        var jsonResultObject = JsonNode.Parse(jsonResult);
        var expectedJsonObject = JsonNode.Parse(expectedJson);

        // Assert
        Assert.True(JsonNode.DeepEquals(jsonResultObject, expectedJsonObject), "The serialized JSON does not match the expected JSON.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SerializeAsV1_1_WithMissingReference_ShouldThrow(string? missingReference)
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
        var exception = Assert.Throws<InvalidOperationException>(() => reference.SerializeAsV1_1(writer));
        Assert.Contains("cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Deserialize_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "x-$ref": "#/components/actions/errorResponse",
            "x-parameterValues": {
                "region": "us",
                "stage": "dev"
            },
            "target": "$.paths['/pets'].get.responses",
            "description": "Override Description",
            "remove": false,
            "copy": "$.paths['/pets'].post.responses",
            "update": {
                "404": {
                    "description": "Not found"
                }
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var reference = OverlayV1_1Deserializer.LoadReusableActionReference(parseNode);

        // Assert
        Assert.NotNull(reference.Reference);
        Assert.Equal("errorResponse", reference.Reference.Id);
        Assert.Equal("#/components/actions/errorResponse", reference.Reference.Reference);
        Assert.NotNull(reference.Reference.ParameterValues);
        Assert.Equal("us", reference.Reference.ParameterValues["region"]);
        Assert.Equal("dev", reference.Reference.ParameterValues["stage"]);
        Assert.Equal("$.paths['/pets'].get.responses", reference.Target);
        Assert.Equal("Override Description", reference.Description);
        Assert.False(reference.Remove);
        Assert.Equal("$.paths['/pets'].post.responses", reference.Copy);
        Assert.Equal("Not found", reference.Update?["404"]?["description"]?.GetValue<string>());
    }

    [Fact]
    public void Deserialize_WithNonStringParameterValue_ShouldCoerceToString()
    {
        // Arrange
        var json = """
        {
            "x-$ref": "#/components/actions/errorResponse",
            "x-parameterValues": {
                "region": false
            }
        }
        """;
        var jsonNode = JsonNode.Parse(json)!;
        var parsingContext = new ParsingContext(new());
        var parseNode = new MapNode(parsingContext, jsonNode);

        // Act
        var reference = OverlayV1_1Deserializer.LoadReusableActionReference(parseNode);

        // Assert
        Assert.NotNull(reference.Reference.ParameterValues);
        Assert.Equal("False", reference.Reference.ParameterValues["region"]);
        Assert.Empty(parsingContext.Diagnostic.Errors);
    }

    [Fact]
    public void ConstructorWithHostDocument_ShouldResolveTargetActionAndApplyFallbackRules()
    {
        // Arrange
        var resolvedAction = new OverlayReusableAction
        {
            Fields = new OverlayAction
            {
                Target = "$.paths['/pets'].get.responses",
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
            Fields = new OverlayAction
            {
                Target = "$.paths['/pets'].get.responses",
                Description = "Resolved reusable action"
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

        // Act
        var reference = new OverlayReusableActionReference("#/components/actions/errorResponse", hostDocument);

        // Assert
        Assert.Equal("errorResponse", reference.Reference.Id);
        Assert.Equal("#/components/actions/errorResponse", reference.Reference.Reference);
        Assert.Same(resolvedAction, reference.TargetAction);
    }

    [Fact]
    public void ResolveParameterValues_ShouldReturnResolvedValuesAndLookupCollections()
    {
        // Arrange
        var regionValue = "us";
        var unknownValue = "x";
        var stageDefault = "dev";
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                ParameterValues = new Dictionary<string, string>
                {
                    ["region"] = regionValue,
                    ["unknown"] = unknownValue
                }
            },
            TargetAction = new OverlayReusableAction
            {
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = "region" },
                    new OverlayReusableActionParameter { Name = "stage", Default = stageDefault },
                    new OverlayReusableActionParameter { Name = "tenant" }
                ]
            }
        };

        // Act
        var (resolvedParameterValues, undefinedParameterValues, missingRequiredParameterValues) = reference.ResolveParameterValues();

        // Assert
        Assert.Equal(2, resolvedParameterValues.Count);
        Assert.Equal(regionValue, resolvedParameterValues["region"]);
        Assert.Equal(stageDefault, resolvedParameterValues["stage"]);
        Assert.True(undefinedParameterValues.SetEquals(["unknown"]));
        Assert.True(missingRequiredParameterValues.SetEquals(["tenant"]));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1invalid")]
    [InlineData("invalid-name")]
    public void ResolveParameterValues_WithInvalidParameterDefinitionName_ShouldThrow(string? definitionName)
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem(),
            TargetAction = new OverlayReusableAction
            {
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = definitionName }
                ]
            }
        };

        // Act + Assert
        var exception = Assert.Throws<InvalidOperationException>(() => reference.ResolveParameterValues());
        Assert.Contains("parameter", exception.Message);
    }

    [Theory]
    [InlineData("_region")]
    [InlineData("region_name")]
    [InlineData("_region_name_1")]
    public void ResolveParameterValues_WithUnderscoreInParameterDefinitionName_ShouldResolve(string definitionName)
    {
        // Arrange
        var value = "us";
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                ParameterValues = new Dictionary<string, string>
                {
                    [definitionName] = value
                }
            },
            TargetAction = new OverlayReusableAction
            {
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = definitionName }
                ]
            }
        };

        // Act
        var (resolvedParameterValues, undefinedParameterValues, missingRequiredParameterValues) = reference.ResolveParameterValues();

        // Assert
        Assert.Single(resolvedParameterValues);
        Assert.Equal(value, resolvedParameterValues[definitionName]);
        Assert.Empty(undefinedParameterValues);
        Assert.Empty(missingRequiredParameterValues);
    }

    [Fact]
    public void ResolveParameterValues_WithDuplicateParameterDefinitionNames_ShouldThrow()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem(),
            TargetAction = new OverlayReusableAction
            {
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = "region" },
                    new OverlayReusableActionParameter { Name = "region" }
                ]
            }
        };

        // Act + Assert
        var exception = Assert.Throws<InvalidOperationException>(() => reference.ResolveParameterValues());
        Assert.Contains("Duplicate reusable action parameter definition name 'region'.", exception.Message);
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
                ParameterValues = new Dictionary<string, string>
                {
                    ["region"] = "us"
                },
                Target = "$.paths['/pets'].get.responses"
            },
            Description = "Resolved reusable action",
            Remove = false,
            Update = JsonNode.Parse("""{ "x-region": "overridden" }"""),
            Copy = "$.paths['/pets'].post.responses",
            TargetAction = new OverlayReusableAction
            {
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = "region" }
                ],
                EnvironmentVariables =
                [
                    new OverlayReusableActionParameter { Name = "STAGE", Default = "dev" }
                ]
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();
        var environmentVariableValues = new Dictionary<string, string>
        {
            ["STAGE"] = "prod"
        };

        // Act
        var resolvedAction = reference.GetResolvedAction(overlayDiagnostic, environmentVariableValues);

        // Assert
        Assert.NotNull(resolvedAction);
        Assert.Equal("$.paths['/pets'].get.responses", resolvedAction.Target);
        Assert.Equal("Resolved reusable action", resolvedAction.Description);
        Assert.False(resolvedAction.Remove);
        Assert.Equal("overridden", resolvedAction.Update?["x-region"]?.GetValue<string>());
        Assert.Equal("$.paths['/pets'].post.responses", resolvedAction.Copy);
        Assert.Empty(overlayDiagnostic.Errors);
    }

    [Fact]
    public void GetResolvedAction_WithInheritedStringPlaceholders_ShouldReplaceValues()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                ParameterValues = new Dictionary<string, string>
                {
                    ["region"] = "eu"
                }
            },
            TargetAction = new OverlayReusableAction
            {
                Fields = new OverlayAction
                {
                    Target = "$.paths['/%param.region%/%env.STAGE%']",
                    Description = "Deploy %param.region% to %env.STAGE%",
                    Copy = "$.copy.%param.region%"
                },
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = "region" }
                ],
                EnvironmentVariables =
                [
                    new OverlayReusableActionParameter { Name = "STAGE", Default = "dev" }
                ]
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();
        var environmentVariableValues = new Dictionary<string, string>
        {
            ["STAGE"] = "prod"
        };

        // Act
        var resolvedAction = reference.GetResolvedAction(overlayDiagnostic, environmentVariableValues);

        // Assert
        Assert.NotNull(resolvedAction);
        Assert.Equal("$.paths['/eu/prod']", resolvedAction.Target);
        Assert.Equal("Deploy eu to prod", resolvedAction.Description);
        Assert.Equal("$.copy.eu", resolvedAction.Copy);
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Empty(overlayDiagnostic.Warnings);
    }

    [Fact]
    public void GetResolvedAction_WithDefinedOverrideStringFields_ShouldNotReplaceValues()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                ParameterValues = new Dictionary<string, string>
                {
                    ["region"] = "eu"
                },
                Target = "$.paths['/%param.region%']",
                Description = "Deploy %param.region%",
                Copy = "$.copy.%param.region%"
            },
            TargetAction = new OverlayReusableAction
            {
                Fields = new OverlayAction
                {
                    Target = "$.paths['/%param.region%/%env.STAGE%']",
                    Description = "Deploy %param.region% to %env.STAGE%",
                    Copy = "$.copy.%param.region%"
                },
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = "region" }
                ],
                EnvironmentVariables =
                [
                    new OverlayReusableActionParameter { Name = "STAGE", Default = "dev" }
                ]
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var resolvedAction = reference.GetResolvedAction(overlayDiagnostic, new Dictionary<string, string>());

        // Assert
        Assert.NotNull(resolvedAction);
        Assert.Equal("$.paths['/%param.region%']", resolvedAction.Target);
        Assert.Equal("Deploy %param.region%", resolvedAction.Description);
        Assert.Equal("$.copy.%param.region%", resolvedAction.Copy);
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Empty(overlayDiagnostic.Warnings);
    }

    [Fact]
    public void GetResolvedAction_WithUnresolvedInheritedPlaceholders_ShouldAddWarnings()
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
                Fields = new OverlayAction
                {
                    Target = "$.paths['/%param.unknown%']",
                    Description = "Deploy to %env.Unknown%",
                    Copy = "$.copy.%param.unknown%"
                }
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var resolvedAction = reference.GetResolvedAction(overlayDiagnostic, new Dictionary<string, string>());

        // Assert
        Assert.NotNull(resolvedAction);
        Assert.Equal("$.paths['/%param.unknown%']", resolvedAction.Target);
        Assert.Equal("Deploy to %env.Unknown%", resolvedAction.Description);
        Assert.Equal("$.copy.%param.unknown%", resolvedAction.Copy);
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Equal(3, overlayDiagnostic.Warnings.Count);
        Assert.Contains(overlayDiagnostic.Warnings, static w => w.Message.Contains("%param.unknown%", StringComparison.Ordinal));
        Assert.Contains(overlayDiagnostic.Warnings, static w => w.Message.Contains("%env.Unknown%", StringComparison.Ordinal));
    }

    [Fact]
    public void GetResolvedAction_WithInheritedUpdatePlaceholders_ShouldReplaceValuesRecursively()
    {
        // Arrange
        var regionValue = "eu";
        var replicasDefault = "3";
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                ParameterValues = new Dictionary<string, string>
                {
                    ["region"] = regionValue
                }
            },
            TargetAction = new OverlayReusableAction
            {
                Fields = new OverlayAction
                {
                    Update = JsonNode.Parse("""
                    {
                        "config": {
                            "regionObject": "%param.region%",
                            "stageValue": "%env.STAGE%",
                            "message": "deploy-%param.region%-%env.STAGE%",
                            "nested": [
                                "%param.region%",
                                "prefix-%env.STAGE%",
                                "%param.replicas%"
                            ]
                        }
                    }
                    """)
                },
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = "region" },
                    new OverlayReusableActionParameter { Name = "replicas", Default = replicasDefault }
                ],
                EnvironmentVariables =
                [
                    new OverlayReusableActionParameter { Name = "STAGE", Default = "dev" }
                ]
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();
        var environmentVariableValues = new Dictionary<string, string>
        {
            ["STAGE"] = "prod"
        };

        // Act
        var resolvedAction = reference.GetResolvedAction(overlayDiagnostic, environmentVariableValues);

        // Assert
        Assert.NotNull(resolvedAction);
        Assert.Equal("eu", resolvedAction.Update?["config"]?["regionObject"]?.GetValue<string>());
        Assert.Equal("prod", resolvedAction.Update?["config"]?["stageValue"]?.GetValue<string>());
        Assert.Equal("deploy-eu-prod", resolvedAction.Update?["config"]?["message"]?.GetValue<string>());
        Assert.Equal("eu", resolvedAction.Update?["config"]?["nested"]?[0]?.GetValue<string>());
        Assert.Equal("prefix-prod", resolvedAction.Update?["config"]?["nested"]?[1]?.GetValue<string>());
        Assert.Equal("3", resolvedAction.Update?["config"]?["nested"]?[2]?.GetValue<string>());
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Empty(overlayDiagnostic.Warnings);
    }

    [Fact]
    public void GetResolvedAction_WithInheritedUpdateUnresolvedPlaceholders_ShouldAddWarning()
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
                Fields = new OverlayAction
                {
                    Update = JsonNode.Parse("""
                    {
                        "config": {
                            "missing": "%param.unknown%",
                            "message": "prefix-%env.unknown%"
                        }
                    }
                    """)
                }
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var resolvedAction = reference.GetResolvedAction(overlayDiagnostic, new Dictionary<string, string>());

        // Assert
        Assert.NotNull(resolvedAction);
        Assert.Equal("%param.unknown%", resolvedAction.Update?["config"]?["missing"]?.GetValue<string>());
        Assert.Equal("prefix-%env.unknown%", resolvedAction.Update?["config"]?["message"]?.GetValue<string>());
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Single(overlayDiagnostic.Warnings);
        Assert.Contains(OverlayConstants.ActionUpdateFieldName, overlayDiagnostic.Warnings[0].Message, StringComparison.Ordinal);
        Assert.Contains("%param.unknown%", overlayDiagnostic.Warnings[0].Message, StringComparison.Ordinal);
        Assert.Contains("%env.unknown%", overlayDiagnostic.Warnings[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetResolvedAction_WithDefinedUpdateOverride_ShouldNotReplaceInheritedUpdatePlaceholders()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                Update = JsonNode.Parse("""{ "from": "override" }"""),
                ParameterValues = new Dictionary<string, string>
                {
                    ["region"] = "eu"
                }
            },
            TargetAction = new OverlayReusableAction
            {
                Fields = new OverlayAction
                {
                    Update = JsonNode.Parse("""{ "template": "%param.region%" }""")
                },
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = "region" }
                ]
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var resolvedAction = reference.GetResolvedAction(overlayDiagnostic, new Dictionary<string, string>());

        // Assert
        Assert.NotNull(resolvedAction);
        Assert.Equal("override", resolvedAction.Update?["from"]?.GetValue<string>());
        Assert.Empty(overlayDiagnostic.Errors);
        Assert.Empty(overlayDiagnostic.Warnings);
    }

    [Fact]
    public void GetResolvedAction_WithUndefinedParameterValues_ShouldAddDiagnosticAndReturnNull()
    {
        // Arrange
        var reference = new OverlayReusableActionReference
        {
            Reference = new OverlayReusableActionReferenceItem
            {
                Id = "errorResponse",
                ParameterValues = new Dictionary<string, string>
                {
                    ["unknown"] = "x"
                }
            },
            TargetAction = new OverlayReusableAction
            {
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = "region" }
                ]
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var resolvedAction = reference.GetResolvedAction(overlayDiagnostic, new Dictionary<string, string>());

        // Assert
        Assert.Null(resolvedAction);
        Assert.Contains(overlayDiagnostic.Errors, static e => e.Message.Contains("undefined parameter values", StringComparison.Ordinal));
        Assert.Contains(overlayDiagnostic.Errors, static e => e.Message.Contains("missing required parameter values", StringComparison.Ordinal));
    }

    [Fact]
    public void GetResolvedAction_WithMissingRequiredEnvironmentVariableValues_ShouldAddDiagnosticAndReturnNull()
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
                Parameters =
                [
                    new OverlayReusableActionParameter { Name = "region", Default = "us" }
                ],
                EnvironmentVariables =
                [
                    new OverlayReusableActionParameter { Name = "STAGE" }
                ]
            }
        };
        var overlayDiagnostic = new OverlayDiagnostic();

        // Act
        var resolvedAction = reference.GetResolvedAction(overlayDiagnostic, new Dictionary<string, string>());

        // Assert
        Assert.Null(resolvedAction);
        Assert.Single(overlayDiagnostic.Errors);
        Assert.Contains("missing required environment variable values", overlayDiagnostic.Errors[0].Message, StringComparison.Ordinal);
        Assert.Equal("/actions/0", overlayDiagnostic.Errors[0].Pointer);
    }
}
#pragma warning restore BOO002