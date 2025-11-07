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
                    Source = ParameterValueSource.Inline,
                    Values = ["dev", "prod"]
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
                    Source = ParameterValueSource.Inline,
                    Values = ["dev", "prod"]
                },
                new OverlayParameter
                {
                    Name = "region",
                    Source = ParameterValueSource.Inline,
                    Values = ["us", "eu"]
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
                    Source = ParameterValueSource.Inline,
                    Values = ["v1", "v2"]
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
                    Source = ParameterValueSource.Inline,
                    Values = ["My API"]
                },
                new OverlayParameter
                {
                    Name = "environment",
                    Source = ParameterValueSource.Inline,
                    Values = ["dev"]
                },
                new OverlayParameter
                {
                    Name = "domain",
                    Source = ParameterValueSource.Inline,
                    Values = ["example"]
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
    public void ExpandActionWithParameters_EnvironmentSource_ReadsFromEnvironment()
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
                        Name = testEnvVar,
                        Source = ParameterValueSource.Environment
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
    public void ExpandActionWithParameters_EnvironmentSourceWithSeparator_SplitsValue()
    {
        // Arrange
        var testEnvVar = "TEST_ENV_VAR_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(testEnvVar, "dev,staging,prod");

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
                        Name = testEnvVar,
                        Source = ParameterValueSource.Environment,
                        Separator = ","
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
    public void ExpandActionWithParameters_EnvironmentSourceNotSet_UsesFallbackValues()
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
                    Source = ParameterValueSource.Environment,
                    Values = ["fallback"]
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
                            Source = ParameterValueSource.Inline,
                            Values = ["production"]
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
}

#pragma warning restore BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.