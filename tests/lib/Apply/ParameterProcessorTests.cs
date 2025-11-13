using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

#pragma warning disable BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class ParameterProcessorTests
{
    [Fact]
    public void ExpandActionWithParameters_NoParameters_ReturnsSingleAction()
    {
        // Arrange
        var action = new OverlayAction
        {
            Target = "$.info.title",
            Update = JsonNode.Parse("\"Test Title\"")
        };

        // Act
        var expanded = ParameterProcessor.ExpandActionWithParameters(action);

        // Assert
        Assert.Single(expanded);
        Assert.Equal("$.info.title", expanded[0].Target);
    }

    [Fact]
    public void ExpandActionWithParameters_SingleParameter_CreatesMultipleActions()
    {
        // Arrange
        var action = new OverlayAction
        {
            Target = "$.info.title",
            Update = JsonNode.Parse("\"API for ${environment}\""),
            Parameters =
            [
                new OverlayParameter
                {
                    Name = "environment",
                    DefaultValues = JsonNode.Parse("""["dev", "prod"]""")
                }
            ]
        };

        // Act
        var expanded = ParameterProcessor.ExpandActionWithParameters(action);

        // Assert
        Assert.Equal(2, expanded.Count);
        Assert.Equal("API for dev", expanded[0].Update?.GetValue<string>());
        Assert.Equal("API for prod", expanded[1].Update?.GetValue<string>());
    }

    [Fact]
    public void ExpandActionWithParameters_MultipleParameters_CreatesCartesianProduct()
    {
        // Arrange
        var action = new OverlayAction
        {
            Target = "$.info.title",
            Update = JsonNode.Parse("\"API for ${environment} in ${region}\""),
            Parameters =
            [
                new OverlayParameter
                {
                    Name = "environment",
                    DefaultValues = JsonNode.Parse("""["dev", "prod"]""")
                },
                new OverlayParameter
                {
                    Name = "region",
                    DefaultValues = JsonNode.Parse("""["us", "eu"]""")
                }
            ]
        };

        // Act
        var expanded = ParameterProcessor.ExpandActionWithParameters(action);

        // Assert
        Assert.Equal(4, expanded.Count);
        Assert.Equal("API for dev in us", expanded[0].Update?.GetValue<string>());
        Assert.Equal("API for dev in eu", expanded[1].Update?.GetValue<string>());
        Assert.Equal("API for prod in us", expanded[2].Update?.GetValue<string>());
        Assert.Equal("API for prod in eu", expanded[3].Update?.GetValue<string>());
    }

    [Fact]
    public void ExpandActionWithParameters_InterpolatesTarget()
    {
        // Arrange
        var action = new OverlayAction
        {
            Target = "$.paths./api/${version}/users",
            Update = JsonNode.Parse("{}"),
            Parameters =
            [
                new OverlayParameter
                {
                    Name = "version",
                    DefaultValues = JsonNode.Parse("""["v1", "v2"]""")
                }
            ]
        };

        // Act
        var expanded = ParameterProcessor.ExpandActionWithParameters(action);

        // Assert
        Assert.Equal(2, expanded.Count);
        Assert.Equal("$.paths./api/v1/users", expanded[0].Target);
        Assert.Equal("$.paths./api/v2/users", expanded[1].Target);
    }

    [Fact]
    public void ExpandActionWithParameters_InterpolatesNestedJson()
    {
        // Arrange
        var action = new OverlayAction
        {
            Target = "$.info",
            Update = JsonNode.Parse("""
            {
                "title": "${title}",
                "description": "API for ${environment}",
                "contact": {
                    "email": "contact@${domain}.com"
                }
            }
            """),
            Parameters =
            [
                new OverlayParameter
                {
                    Name = "title",
                    DefaultValues = JsonNode.Parse("""["My API"]""")
                },
                new OverlayParameter
                {
                    Name = "environment",
                    DefaultValues = JsonNode.Parse("""["dev"]""")
                },
                new OverlayParameter
                {
                    Name = "domain",
                    DefaultValues = JsonNode.Parse("""["example"]""")
                }
            ]
        };

        // Act
        var expanded = ParameterProcessor.ExpandActionWithParameters(action);

        // Assert
        Assert.Single(expanded);
        var update = expanded[0].Update?.AsObject();
        Assert.Equal("My API", update?["title"]?.GetValue<string>());
        Assert.Equal("API for dev", update?["description"]?.GetValue<string>());
        Assert.Equal("contact@example.com", update?["contact"]?["email"]?.GetValue<string>());
    }

    [Fact]
    public void ExpandActionWithParameters_ReadsFromEnvironment()
    {
        // Arrange
        var testEnvVar = "TEST_ENV_VAR_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(testEnvVar, "test-value");

        try
        {
            var action = new OverlayAction
            {
                Target = "$.info.title",
                Update = JsonNode.Parse($"\"API for ${{{testEnvVar}}}\""),
                Parameters =
                [
                    new OverlayParameter
                    {
                        Name = testEnvVar
                    }
                ]
            };

            // Act
            var expanded = ParameterProcessor.ExpandActionWithParameters(action);

            // Assert
            Assert.Single(expanded);
            Assert.Equal("API for test-value", expanded[0].Update?.GetValue<string>());
        }
        finally
        {
            Environment.SetEnvironmentVariable(testEnvVar, null);
        }
    }

    [Fact]
    public void ExpandActionWithParameters_EnvironmentNotSet_UsesDefaultValues()
    {
        // Arrange
        var testEnvVar = "NONEXISTENT_ENV_VAR_" + Guid.NewGuid().ToString("N");

        var action = new OverlayAction
        {
            Target = "$.info.title",
            Update = JsonNode.Parse($"\"API for ${{{testEnvVar}}}\""),
            Parameters =
            [
                new OverlayParameter
                {
                    Name = testEnvVar,
                    DefaultValues = JsonNode.Parse("""["fallback"]""")
                }
            ]
        };

        // Act
        var expanded = ParameterProcessor.ExpandActionWithParameters(action);

        // Assert
        Assert.Single(expanded);
        Assert.Equal("API for fallback", expanded[0].Update?.GetValue<string>());
    }

    [Fact]
    public void ApplyOverlayWithParameters_IntegrationTest()
    {
        // Arrange
        var overlayDocument = new OverlayDocument
        {
            Info = new OverlayInfo
            {
                Title = "Test Overlay",
                Version = "1.0.0"
            },
            Extends = "test.json",
            Actions =
            [
                new OverlayAction
                {
                    Target = "$.info.title",
                    Update = JsonNode.Parse("\"API for ${environment}\""),
                    Parameters =
                    [
                        new OverlayParameter
                        {
                            Name = "environment",
                            DefaultValues = JsonNode.Parse("""["production"]""")
                        }
                    ]
                }
            ]
        };

        var document = JsonNode.Parse("""
        {
            "openapi": "3.0.0",
            "info": {
                "title": "Original Title",
                "version": "1.0.0"
            }
        }
        """)!;

        var diagnostic = new OverlayDiagnostic();

        // Act
        var result = overlayDocument.ApplyToDocument(document, diagnostic);

        // Assert
        Assert.True(result);
        Assert.Empty(diagnostic.Errors);
        Assert.Equal("API for production", document["info"]?["title"]?.GetValue<string>());
    }

    [Fact]
    public void ExpandActionWithParameters_EnvironmentVariableWithJsonArray_ValidatesAndExpands()
    {
        // Arrange
        var testEnvVar = "TEST_ENV_VAR_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(testEnvVar, """["dev", "staging", "prod"]""");

        try
        {
            var action = new OverlayAction
            {
                Target = "$.info.title",
                Update = JsonNode.Parse($"\"API for ${{{testEnvVar}}}\""),
                Parameters =
                [
                    new OverlayParameter
                    {
                        Name = testEnvVar
                    }
                ]
            };

            // Act
            var expanded = ParameterProcessor.ExpandActionWithParameters(action);

            // Assert
            Assert.Equal(3, expanded.Count);
            Assert.Equal("API for dev", expanded[0].Update?.GetValue<string>());
            Assert.Equal("API for staging", expanded[1].Update?.GetValue<string>());
            Assert.Equal("API for prod", expanded[2].Update?.GetValue<string>());
        }
        finally
        {
            Environment.SetEnvironmentVariable(testEnvVar, null);
        }
    }

    [Fact]
    public void ExpandActionWithParameters_EnvironmentVariableWithInvalidJson_ThrowsException()
    {
        // Arrange
        var testEnvVar = "TEST_ENV_VAR_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(testEnvVar, """[123, 456]"""); // Invalid: numbers instead of strings

        try
        {
            var action = new OverlayAction
            {
                Target = "$.info.title",
                Update = JsonNode.Parse($"\"API for ${{{testEnvVar}}}\""),
                Parameters =
                [
                    new OverlayParameter
                    {
                        Name = testEnvVar
                    }
                ]
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ParameterProcessor.ExpandActionWithParameters(action));
        }
        finally
        {
            Environment.SetEnvironmentVariable(testEnvVar, null);
        }
    }

    [Fact]
    public void ExpandActionWithParameters_DottedNotation_ExtractsObjectProperty()
    {
        // Arrange
        var action = new OverlayAction
        {
            Target = "$.info.title",
            Update = JsonNode.Parse("\"API - ${server.url}\""),
            Parameters =
            [
                new OverlayParameter
                {
                    Name = "server",
                    DefaultValues = JsonNode.Parse("""[{"url": "https://api1.example.com", "name": "Server 1"}, {"url": "https://api2.example.com", "name": "Server 2"}]""")
                }
            ]
        };

        // Act
        var expanded = ParameterProcessor.ExpandActionWithParameters(action);

        // Assert
        Assert.Equal(2, expanded.Count);
        Assert.Equal("API - https://api1.example.com", expanded[0].Update?.GetValue<string>());
        Assert.Equal("API - https://api2.example.com", expanded[1].Update?.GetValue<string>());
    }

    [Fact]
    public void ExpandActionWithParameters_DottedNotationWithMultipleProperties_ExtractsCorrectly()
    {
        // Arrange
        var action = new OverlayAction
        {
            Target = "$.info.title",
            Update = JsonNode.Parse("\"${server.name} at ${server.url}\""),
            Parameters =
            [
                new OverlayParameter
                {
                    Name = "server",
                    DefaultValues = JsonNode.Parse("""[{"url": "https://api.example.com", "name": "Production"}]""")
                }
            ]
        };

        // Act
        var expanded = ParameterProcessor.ExpandActionWithParameters(action);

        // Assert
        Assert.Single(expanded);
        Assert.Equal("Production at https://api.example.com", expanded[0].Update?.GetValue<string>());
    }

    [Fact]
    public void ExpandActionWithParameters_DottedNotationWithMissingKey_KeepsPlaceholder()
    {
        // Arrange
        var action = new OverlayAction
        {
            Target = "$.info.title",
            Update = JsonNode.Parse("\"API - ${server.missingKey}\""),
            Parameters =
            [
                new OverlayParameter
                {
                    Name = "server",
                    DefaultValues = JsonNode.Parse("""[{"url": "https://api.example.com"}]""")
                }
            ]
        };

        // Act
        var expanded = ParameterProcessor.ExpandActionWithParameters(action);

        // Assert
        Assert.Single(expanded);
        Assert.Equal("API - ${server.missingKey}", expanded[0].Update?.GetValue<string>());
    }

    [Fact]
    public void ExpandActionWithParameters_EnvironmentVariableWithObjects_ValidatesAndSupportsDottedNotation()
    {
        // Arrange
        var testEnvVar = "TEST_ENV_VAR_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(testEnvVar, """[{"url": "https://env-api.com", "region": "us-east"}]""");

        try
        {
            var action = new OverlayAction
            {
                Target = "$.info.title",
                Update = JsonNode.Parse($"\"API in ${{{testEnvVar}.region}} at ${{{testEnvVar}.url}}\""),
                Parameters =
                [
                    new OverlayParameter
                    {
                        Name = testEnvVar
                    }
                ]
            };

            // Act
            var expanded = ParameterProcessor.ExpandActionWithParameters(action);

            // Assert
            Assert.Single(expanded);
            Assert.Equal("API in us-east at https://env-api.com", expanded[0].Update?.GetValue<string>());
        }
        finally
        {
            Environment.SetEnvironmentVariable(testEnvVar, null);
        }
    }
}

#pragma warning restore BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.