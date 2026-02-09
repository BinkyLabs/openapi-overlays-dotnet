using System.Text.Json.Nodes;
using BinkyLabs.OpenApi.Overlays.Generation;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class OverlayGeneratorTests
{
    [Fact]
    public void Generate_WithSimplePropertyChange_GeneratesUpdateAction()
    {
        var source = new JsonObject { ["info"] = new JsonObject { ["title"] = "Old API", ["version"] = "1.0.0" } };
        var target = new JsonObject { ["info"] = new JsonObject { ["title"] = "New API", ["version"] = "1.0.0" } };

        var result = OverlayGenerator.Generate(source, target);

        Assert.NotNull(result.Document);
        Assert.Empty(result.Diagnostic.Errors);
        Assert.NotNull(result.Document.Actions);
        Assert.Single(result.Document.Actions);
        Assert.Equal("$.info.title", result.Document.Actions[0].Target);
        Assert.NotNull(result.Document.Actions[0].Update);
    }

    [Fact]
    public void Generate_WithPropertyRemoval_GeneratesRemoveAction()
    {
        var source = new JsonObject { ["info"] = new JsonObject { ["title"] = "API", ["description"] = "Some desc" } };
        var target = new JsonObject { ["info"] = new JsonObject { ["title"] = "API" } };

        var result = OverlayGenerator.Generate(source, target);

        Assert.NotNull(result.Document);
        Assert.Empty(result.Diagnostic.Errors);
        Assert.NotNull(result.Document.Actions);
        var removeAction = result.Document.Actions.FirstOrDefault(a => a.Remove == true);
        Assert.NotNull(removeAction);
        Assert.Equal("$.info.description", removeAction.Target);
    }

    [Fact]
    public void Generate_WithPropertyAddition_GeneratesUpdateAction()
    {
        var source = new JsonObject { ["info"] = new JsonObject { ["title"] = "API" } };
        var target = new JsonObject { ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } };

        var result = OverlayGenerator.Generate(source, target);

        Assert.NotNull(result.Document);
        Assert.Empty(result.Diagnostic.Errors);
        Assert.NotNull(result.Document.Actions);
        var addAction = result.Document.Actions.FirstOrDefault(a => a.Update != null && a.Target == "$.info");
        Assert.NotNull(addAction);
    }

    [Fact]
    public void Generate_WithIdenticalDocuments_GeneratesNoActions()
    {
        var source = new JsonObject { ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } };
        var target = new JsonObject { ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } };

        var result = OverlayGenerator.Generate(source, target);

        Assert.NotNull(result.Document);
        Assert.Empty(result.Diagnostic.Errors);
        Assert.NotNull(result.Document.Actions);
        Assert.Empty(result.Document.Actions);
    }

    [Fact]
    public async Task GenerateAsync_WithFilePaths_GeneratesOverlay()
    {
        var sourcePath = Path.Combine("..", "..", "..", "..", "..", "examples", "diff", "source-matching.json");
        var targetPath = Path.Combine("..", "..", "..", "..", "..", "examples", "diff", "target-matching.json");
        
        var result = await OverlayGenerator.GenerateAsync(sourcePath, targetPath);

        Assert.NotNull(result.Document);
        Assert.Empty(result.Diagnostic.Errors);
        Assert.NotNull(result.Document.Actions);
        Assert.NotEmpty(result.Document.Actions);
    }

    [Fact]
    public void Generate_WithMatchingVersions_GeneratesOverlay()
    {
        var source = new JsonObject 
        { 
            ["openapi"] = "3.0.0",
            ["info"] = new JsonObject { ["title"] = "Old API", ["version"] = "1.0.0" } 
        };
        var target = new JsonObject 
        { 
            ["openapi"] = "3.0.0",
            ["info"] = new JsonObject { ["title"] = "New API", ["version"] = "1.0.0" } 
        };

        var result = OverlayGenerator.Generate(source, target);

        Assert.NotNull(result.Document);
        Assert.Empty(result.Diagnostic.Errors);
        Assert.NotNull(result.Document.Actions);
    }

    [Fact]
    public void Generate_WithMismatchedVersions_ReturnsError()
    {
        var source = new JsonObject 
        { 
            ["openapi"] = "3.0.0",
            ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } 
        };
        var target = new JsonObject 
        { 
            ["openapi"] = "3.1.0",
            ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } 
        };

        var result = OverlayGenerator.Generate(source, target);

        Assert.Null(result.Document);
        Assert.NotEmpty(result.Diagnostic.Errors);
        Assert.Single(result.Diagnostic.Errors);
        Assert.Contains("3.0.0", result.Diagnostic.Errors[0].Message);
        Assert.Contains("3.1.0", result.Diagnostic.Errors[0].Message);
    }

    [Fact]
    public void Generate_WithMissingSourceVersion_ReturnsError()
    {
        var source = new JsonObject 
        { 
            ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } 
        };
        var target = new JsonObject 
        { 
            ["openapi"] = "3.0.0",
            ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } 
        };

        var result = OverlayGenerator.Generate(source, target);

        Assert.Null(result.Document);
        Assert.NotEmpty(result.Diagnostic.Errors);
        Assert.Single(result.Diagnostic.Errors);
        Assert.Contains("missing", result.Diagnostic.Errors[0].Message);
    }

    [Fact]
    public void Generate_WithMissingTargetVersion_ReturnsError()
    {
        var source = new JsonObject 
        { 
            ["openapi"] = "3.0.0",
            ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } 
        };
        var target = new JsonObject 
        { 
            ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } 
        };

        var result = OverlayGenerator.Generate(source, target);

        Assert.Null(result.Document);
        Assert.NotEmpty(result.Diagnostic.Errors);
        Assert.Single(result.Diagnostic.Errors);
        Assert.Contains("missing", result.Diagnostic.Errors[0].Message);
    }

    [Fact]
    public void Generate_WithBothVersionsMissing_DoesNotError()
    {
        var source = new JsonObject 
        { 
            ["info"] = new JsonObject { ["title"] = "Old API", ["version"] = "1.0.0" } 
        };
        var target = new JsonObject 
        { 
            ["info"] = new JsonObject { ["title"] = "New API", ["version"] = "1.0.0" } 
        };

        var result = OverlayGenerator.Generate(source, target);

        Assert.NotNull(result.Document);
        Assert.Empty(result.Diagnostic.Errors);
    }

    [Fact]
    public async Task GenerateAsync_WithMismatchedVersionsInFiles_ReturnsError()
    {
        var sourcePath = Path.Combine("..", "..", "..", "..", "..", "examples", "diff", "source-v1.json");
        var targetPath = Path.Combine("..", "..", "..", "..", "..", "examples", "diff", "target-v2-openapi31.json");

        var result = await OverlayGenerator.GenerateAsync(sourcePath, targetPath);

        Assert.Null(result.Document);
        Assert.NotEmpty(result.Diagnostic.Errors);
        Assert.Contains("3.0.0", result.Diagnostic.Errors[0].Message);
        Assert.Contains("3.1.0", result.Diagnostic.Errors[0].Message);
    }

    [Fact]
    public async Task GenerateAsync_WithMatchingVersionsInFiles_Succeeds()
    {
        var sourcePath = Path.Combine("..", "..", "..", "..", "..", "examples", "diff", "source-matching.json");
        var targetPath = Path.Combine("..", "..", "..", "..", "..", "examples", "diff", "target-matching.json");

        var result = await OverlayGenerator.GenerateAsync(sourcePath, targetPath);

        Assert.NotNull(result.Document);
        Assert.Empty(result.Diagnostic.Errors);
        Assert.NotNull(result.Document.Actions);
    }
}
