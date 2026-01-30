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

        var overlay = OverlayGenerator.Generate(source, target);

        Assert.NotNull(overlay);
        Assert.NotNull(overlay.Actions);
        Assert.Single(overlay.Actions);
        Assert.Equal("$.info.title", overlay.Actions[0].Target);
        Assert.NotNull(overlay.Actions[0].Update);
    }

    [Fact]
    public void Generate_WithPropertyRemoval_GeneratesRemoveAction()
    {
        var source = new JsonObject { ["info"] = new JsonObject { ["title"] = "API", ["description"] = "Some desc" } };
        var target = new JsonObject { ["info"] = new JsonObject { ["title"] = "API" } };

        var overlay = OverlayGenerator.Generate(source, target);

        Assert.NotNull(overlay);
        Assert.NotNull(overlay.Actions);
        var removeAction = overlay.Actions.FirstOrDefault(a => a.Remove == true);
        Assert.NotNull(removeAction);
        Assert.Equal("$.info.description", removeAction.Target);
    }

    [Fact]
    public void Generate_WithPropertyAddition_GeneratesUpdateAction()
    {
        var source = new JsonObject { ["info"] = new JsonObject { ["title"] = "API" } };
        var target = new JsonObject { ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } };

        var overlay = OverlayGenerator.Generate(source, target);

        Assert.NotNull(overlay);
        Assert.NotNull(overlay.Actions);
        var addAction = overlay.Actions.FirstOrDefault(a => a.Update != null && a.Target == "$.info");
        Assert.NotNull(addAction);
    }

    [Fact]
    public void Generate_WithIdenticalDocuments_GeneratesNoActions()
    {
        var source = new JsonObject { ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } };
        var target = new JsonObject { ["info"] = new JsonObject { ["title"] = "API", ["version"] = "1.0.0" } };

        var overlay = OverlayGenerator.Generate(source, target);

        Assert.NotNull(overlay);
        Assert.NotNull(overlay.Actions);
        Assert.Empty(overlay.Actions);
    }

    [Fact]
    public async Task GenerateAsync_WithFilePaths_GeneratesOverlay()
    {
        var sourcePath = Path.Combine("..", "..", "..", "..", "..", "examples", "diff", "source-v1.json");
        var targetPath = Path.Combine("..", "..", "..", "..", "..", "examples", "diff", "target-v2.json");
        
        var overlay = await OverlayGenerator.GenerateAsync(sourcePath, targetPath);

        Assert.NotNull(overlay);
        Assert.NotNull(overlay.Actions);
        Assert.NotEmpty(overlay.Actions);
    }
}
