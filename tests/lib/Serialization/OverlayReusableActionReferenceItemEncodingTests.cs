namespace BinkyLabs.OpenApi.Overlays.Tests;

#pragma warning disable BOO002
public class OverlayReusableActionReferenceItemEncodingTests
{
    // EncodeJsonPointerToken tests

    [Theory]
    [InlineData("simple", "simple")]
    [InlineData("my/action", "my~1action")]
    [InlineData("my~action", "my~0action")]
    [InlineData("my/~action", "my~1~0action")]
    [InlineData("a~1b", "a~01b")]           // tilde followed by '1' must not become ~1
    [InlineData("a~~b", "a~0~0b")]
    [InlineData("a/b/c", "a~1b~1c")]
    public void EncodeJsonPointerToken_ShouldEncodeCorrectly(string input, string expected)
    {
        var result = OverlayReusableActionReferenceItem.EncodeJsonPointerToken(input);
        Assert.Equal(expected, result);
    }

    // DecodeJsonPointerToken tests

    [Theory]
    [InlineData("simple", "simple")]
    [InlineData("my~1action", "my/action")]
    [InlineData("my~0action", "my~action")]
    [InlineData("my~1~0action", "my/~action")]
    [InlineData("a~01b", "a~1b")]           // ~0 followed by '1' must not become /
    [InlineData("a~0~0b", "a~~b")]
    [InlineData("a~1b~1c", "a/b/c")]
    public void DecodeJsonPointerToken_ShouldDecodeCorrectly(string input, string expected)
    {
        var result = OverlayReusableActionReferenceItem.DecodeJsonPointerToken(input);
        Assert.Equal(expected, result);
    }

    // Round-trip tests

    [Theory]
    [InlineData("simple")]
    [InlineData("my/action")]
    [InlineData("my~action")]
    [InlineData("my/~action")]
    [InlineData("a/b/c")]
    [InlineData("a~~b")]
    public void EncodeAndDecodeJsonPointerToken_ShouldRoundTrip(string original)
    {
        var encoded = OverlayReusableActionReferenceItem.EncodeJsonPointerToken(original);
        var decoded = OverlayReusableActionReferenceItem.DecodeJsonPointerToken(encoded);
        Assert.Equal(original, decoded);
    }

    // Reference property tests

    [Theory]
    [InlineData("simple", "#/components/actions/simple")]
    [InlineData("my/action", "#/components/actions/my~1action")]
    [InlineData("my~action", "#/components/actions/my~0action")]
    [InlineData("my/~action", "#/components/actions/my~1~0action")]
    public void Reference_ShouldEncodeIdCorrectly(string id, string expectedReference)
    {
        var item = new OverlayReusableActionReferenceItem(id);
        Assert.Equal(expectedReference, item.Reference);
    }

    // NormalizeReusableActionReferenceId tests

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("simple", "simple")]
    [InlineData("#/components/actions/simple", "simple")]
    [InlineData("#/components/actions/my~1action", "my/action")]
    [InlineData("#/components/actions/my~0action", "my~action")]
    [InlineData("#/components/actions/my~1~0action", "my/~action")]
    public void NormalizeReusableActionReferenceId_ShouldDecodeCorrectly(string? input, string? expected)
    {
        var result = OverlayReusableActionReferenceItem.NormalizeReusableActionReferenceId(input);
        Assert.Equal(expected, result);
    }

    // Id setter normalizes the value

    [Fact]
    public void Id_WhenSetWithEncodedReference_ShouldStoreDecodedId()
    {
        var item = new OverlayReusableActionReferenceItem
        {
            Id = "#/components/actions/my~1action"
        };
        Assert.Equal("my/action", item.Id);
        Assert.Equal("#/components/actions/my~1action", item.Reference);
    }

    [Fact]
    public void Id_WhenSetWithDecodedIdentifier_ShouldStoreItAsIs()
    {
        var item = new OverlayReusableActionReferenceItem
        {
            Id = "my/action"
        };
        Assert.Equal("my/action", item.Id);
        Assert.Equal("#/components/actions/my~1action", item.Reference);
    }
}
#pragma warning restore BOO002