using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

namespace BinkyLabs.OpenApi.Overlays.Tests;

/// <summary>
/// Tests for critical OverlayAction scenarios that were fixed to ensure they don't regress.
/// </summary>
public class OverlayActionRegressionTests
{
    [Fact]
    public void ApplyToDocument_ArrayUpdate_ShouldAppendNotReplace()
    {
        // Per Overlay spec, updating an array directly should append items
        // Arrange
        var document = JsonNode.Parse(@"{
            ""tags"": [
                { ""name"": ""tag1"" },
                { ""name"": ""tag2"" }
            ]
        }")!;

        var action = new OverlayAction
        {
            Target = "$.tags",
            Update = JsonNode.Parse(@"[ { ""name"": ""tag3"" } ]")
        };

        var diagnostic = new OverlayDiagnostic();

        // Act
        var result = action.ApplyToDocument(document, diagnostic, 0);

        // Assert
        Assert.True(result);
        Assert.Empty(diagnostic.Errors);

        var tags = document["tags"] as JsonArray;
        Assert.NotNull(tags);
        Assert.Equal(3, tags.Count); // Should have 3 items (appended, not replaced)
        Assert.Equal("tag1", tags[0]!["name"]!.GetValue<string>());
        Assert.Equal("tag2", tags[1]!["name"]!.GetValue<string>());
        Assert.Equal("tag3", tags[2]!["name"]!.GetValue<string>());
    }

    [Fact]
    public void ApplyToDocument_ArrayWithinObject_ShouldAppendItems()
    {
        // When updating an object that contains an array, the array items should be appended
        // Arrange
        var document = JsonNode.Parse(@"{
            ""paths"": {
                ""/pets"": {
                    ""get"": {
                        ""tags"": [""existing""]
                    }
                }
            }
        }")!;

        var action = new OverlayAction
        {
            Target = "$.paths['/pets'].get",
            Update = JsonNode.Parse(@"{
                ""tags"": [""added""]
            }")
        };

        var diagnostic = new OverlayDiagnostic();

        // Act
        var result = action.ApplyToDocument(document, diagnostic, 0);

        // Assert
        Assert.True(result);
        Assert.Empty(diagnostic.Errors);

        var tags = document["paths"]!["/pets"]!["get"]!["tags"] as JsonArray;
        Assert.NotNull(tags);
        Assert.Equal(2, tags.Count); // Should append
        Assert.Contains(tags, t => t!.GetValue<string>() == "existing");
        Assert.Contains(tags, t => t!.GetValue<string>() == "added");
    }

    [Fact]
    public void ApplyToDocument_ScalarValueUpdate_ShouldReplaceValue()
    {
        // Arrange
        var document = JsonNode.Parse(@"{
            ""info"": {
                ""title"": ""Old Title"",
                ""version"": ""1.0.0""
            }
        }")!;

        var action = new OverlayAction
        {
            Target = "$.info.title",
            Update = JsonValue.Create("New Title")
        };

        var diagnostic = new OverlayDiagnostic();

        // Act
        var result = action.ApplyToDocument(document, diagnostic, 0);

        // Assert
        Assert.True(result);
        Assert.Empty(diagnostic.Errors);

        var title = document["info"]!["title"]!.GetValue<string>();
        Assert.Equal("New Title", title);
    }

    [Fact]
    public void ApplyToDocument_NumberValueUpdate_ShouldReplaceValue()
    {
        // Arrange
        var document = JsonNode.Parse(@"{
            ""schema"": {
                ""minLength"": 5
            }
        }")!;

        var action = new OverlayAction
        {
            Target = "$.schema.minLength",
            Update = JsonValue.Create(10)
        };

        var diagnostic = new OverlayDiagnostic();

        // Act
        var result = action.ApplyToDocument(document, diagnostic, 0);

        // Assert
        Assert.True(result);
        Assert.Empty(diagnostic.Errors);

        var minLength = document["schema"]!["minLength"]!.GetValue<int>();
        Assert.Equal(10, minLength);
    }

    [Fact]
    public void ApplyToDocument_JsonPathWithSlashInPropertyName_ShouldWork()
    {
        // Arrange - Path names starting with / need bracket notation
        var document = JsonNode.Parse(@"{
            ""paths"": {
                ""/pets"": {
                    ""get"": {
                        ""summary"": ""Old summary""
                    }
                }
            }
        }")!;

        var action = new OverlayAction
        {
            Target = "$.paths['/pets'].get.summary",
            Update = JsonValue.Create("New summary")
        };

        var diagnostic = new OverlayDiagnostic();

        // Act
        var result = action.ApplyToDocument(document, diagnostic, 0);

        // Assert
        Assert.True(result);
        Assert.Empty(diagnostic.Errors);

        var summary = document["paths"]!["/pets"]!["get"]!["summary"]!.GetValue<string>();
        Assert.Equal("New summary", summary);
    }

    [Fact]
    public void ApplyToDocument_ObjectMerge_ShouldAddNewProperties()
    {
        // Arrange
        var document = JsonNode.Parse(@"{
            ""info"": {
                ""title"": ""API"",
                ""version"": ""1.0.0""
            }
        }")!;

        var action = new OverlayAction
        {
            Target = "$.info",
            Update = JsonNode.Parse(@"{
                ""description"": ""New description"",
                ""contact"": {
                    ""email"": ""support@example.com""
                }
            }")
        };

        var diagnostic = new OverlayDiagnostic();

        // Act
        var result = action.ApplyToDocument(document, diagnostic, 0);

        // Assert
        Assert.True(result);
        Assert.Empty(diagnostic.Errors);

        var info = document["info"] as JsonObject;
        Assert.NotNull(info);
        
        // Original properties should remain
        Assert.Equal("API", info["title"]!.GetValue<string>());
        Assert.Equal("1.0.0", info["version"]!.GetValue<string>());
        
        // New properties should be added
        Assert.Equal("New description", info["description"]!.GetValue<string>());
        Assert.NotNull(info["contact"]);
        Assert.Equal("support@example.com", info["contact"]!["email"]!.GetValue<string>());
    }

    [Fact]
    public void ApplyToDocument_RemoveAndAdd_ShouldReplaceArray()
    {
        // To replace an array (not append), we use remove + add pattern
        // Arrange
        var document = JsonNode.Parse(@"{
            ""servers"": [
                { ""url"": ""https://old.example.com"" },
                { ""url"": ""https://old2.example.com"" }
            ]
        }")!;

        var removeAction = new OverlayAction
        {
            Target = "$.servers",
            Remove = true
        };

        var addAction = new OverlayAction
        {
            Target = "$",
            Update = JsonNode.Parse(@"{
                ""servers"": [
                    { ""url"": ""https://new.example.com"" }
                ]
            }")
        };

        var diagnostic = new OverlayDiagnostic();

        // Act
        var removeResult = removeAction.ApplyToDocument(document, diagnostic, 0);
        Assert.True(removeResult);
        
        var addResult = addAction.ApplyToDocument(document, diagnostic, 1);
        Assert.True(addResult);

        // Assert
        Assert.Empty(diagnostic.Errors);

        var servers = document["servers"] as JsonArray;
        Assert.NotNull(servers);
        Assert.Single(servers); // Should have only 1 server (replaced, not appended)
        Assert.Equal("https://new.example.com", servers[0]!["url"]!.GetValue<string>());
    }

    [Fact]
    public void ApplyToDocument_NestedPropertyUpdate_ShouldNotAffectSiblings()
    {
        // Arrange
        var document = JsonNode.Parse(@"{
            ""components"": {
                ""schemas"": {
                    ""Pet"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""name"": {
                                ""type"": ""string""
                            },
                            ""age"": {
                                ""type"": ""integer""
                            }
                        }
                    }
                }
            }
        }")!;

        var action = new OverlayAction
        {
            Target = "$.components.schemas.Pet.properties.name",
            Update = JsonNode.Parse(@"{
                ""minLength"": 1,
                ""maxLength"": 100
            }")
        };

        var diagnostic = new OverlayDiagnostic();

        // Act
        var result = action.ApplyToDocument(document, diagnostic, 0);

        // Assert
        Assert.True(result);
        Assert.Empty(diagnostic.Errors);

        var name = document["components"]!["schemas"]!["Pet"]!["properties"]!["name"] as JsonObject;
        Assert.NotNull(name);
        
        // Original property should remain
        Assert.Equal("string", name["type"]!.GetValue<string>());
        
        // New properties should be added
        Assert.Equal(1, name["minLength"]!.GetValue<int>());
        Assert.Equal(100, name["maxLength"]!.GetValue<int>());

        // Sibling property should be unchanged
        var age = document["components"]!["schemas"]!["Pet"]!["properties"]!["age"] as JsonObject;
        Assert.NotNull(age);
        Assert.Equal("integer", age["type"]!.GetValue<string>());
        Assert.False(age.ContainsKey("minLength"));
    }

    [Fact]
    public void ApplyToDocument_TypeMismatch_ShouldReplaceValue()
    {
        // When types don't match (e.g., replacing string with object), should replace
        // Arrange
        var document = JsonNode.Parse(@"{
            ""value"": ""simple string""
        }")!;

        var action = new OverlayAction
        {
            Target = "$.value",
            Update = JsonNode.Parse(@"{
                ""complex"": ""object""
            }")
        };

        var diagnostic = new OverlayDiagnostic();

        // Act
        var result = action.ApplyToDocument(document, diagnostic, 0);

        // Assert
        Assert.True(result);
        Assert.Empty(diagnostic.Errors);

        var value = document["value"] as JsonObject;
        Assert.NotNull(value);
        Assert.Equal("object", value["complex"]!.GetValue<string>());
    }
}
