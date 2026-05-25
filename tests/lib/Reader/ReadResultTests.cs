// Licensed under the MIT license.

using BinkyLabs.OpenApi.Overlays.Reader;

namespace BinkyLabs.OpenApi.Overlays.Tests.Reader;

public class ReadResultTests
{
    [Fact]
    public void Deconstruct_TwoOut_ReturnsDocumentAndDiagnostic()
    {
        var doc = new OverlayDocument();
        var diag = new OverlayDiagnostic();
        var result = new ReadResult { Document = doc, Diagnostic = diag };

        var (d, di) = result;

        Assert.Same(doc, d);
        Assert.Same(diag, di);
    }

    [Fact]
    public void Deconstruct_OneOut_ReturnsDocument()
    {
        var doc = new OverlayDocument();
        var result = new ReadResult { Document = doc };

        result.Deconstruct(out var d);

        Assert.Same(doc, d);
    }
}