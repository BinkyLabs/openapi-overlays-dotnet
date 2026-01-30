using BinkyLabs.OpenApi.Overlays.Generation;

namespace BinkyLabs.OpenApi.Overlays.Tests;

public class JsonPathBuilderTests
{
    [Fact]
    public void BuildPath_WithSimpleProperty_ReturnsCorrectPath()
    {
        var result = JsonPathBuilder.BuildPath("$", "info");
        Assert.Equal("$.info", result);
    }

    [Fact]
    public void BuildPath_WithNestedProperty_ReturnsCorrectPath()
    {
        var result = JsonPathBuilder.BuildPath("$.info", "title");
        Assert.Equal("$.info.title", result);
    }

    [Fact]
    public void BuildPath_WithPropertyContainingSlash_UsesBracketNotation()
    {
        var result = JsonPathBuilder.BuildPath("$.paths", "/users");
        Assert.Equal("$.paths['/users']", result);
    }

    [Fact]
    public void BuildPath_WithArrayIndex_ReturnsCorrectPath()
    {
        var result = JsonPathBuilder.BuildPath("$.servers", 0);
        Assert.Equal("$.servers[0]", result);
    }

    [Fact]
    public void BuildPath_WithPropertyContainingDash_UsesBracketNotation()
    {
        var result = JsonPathBuilder.BuildPath("$", "x-custom");
        Assert.Equal("$['x-custom']", result);
    }
}
