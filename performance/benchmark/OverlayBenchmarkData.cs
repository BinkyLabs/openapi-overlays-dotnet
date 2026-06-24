using BinkyLabs.OpenApi.Overlays;

namespace performance;

internal static class OverlayBenchmarkData
{
    private const string TestsPathSegment = "tests";
    private const string LibraryTestsPathSegment = "lib";
    private const string SpecificationPathSegment = "Specification";
    private const string ExamplesPathSegment = "examples";

    public static async Task<byte[]> ReadExampleBytesAsync(string fileName)
    {
        return await File.ReadAllBytesAsync(GetExamplePath(fileName)).ConfigureAwait(false);
    }

    public static async Task<OverlayDocument> LoadOverlayAsync(byte[] overlayBytes, string format)
    {
        using var stream = new MemoryStream(overlayBytes, writable: false);
        var (document, diagnostic) = await OverlayDocument.LoadFromStreamAsync(stream, format).ConfigureAwait(false);
        if (document is null)
        {
            throw new InvalidOperationException($"Failed to load overlay: {string.Join(Environment.NewLine, diagnostic?.Errors.Select(static e => e.Message) ?? [])}");
        }

        if (diagnostic?.Errors.Count > 0)
        {
            throw new InvalidOperationException($"Overlay contains diagnostics: {string.Join(Environment.NewLine, diagnostic.Errors.Select(static e => e.Message))}");
        }

        return document;
    }

    private static string GetExamplePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Example file name must be a non-empty relative path.", nameof(fileName));
        }

        if (Path.IsPathRooted(fileName))
        {
            throw new ArgumentException("Example file name must be a relative path.", nameof(fileName));
        }

        return Path.Combine(GetRepositoryRoot(), TestsPathSegment, LibraryTestsPathSegment, SpecificationPathSegment, ExamplesPathSegment, fileName);
    }

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Join(current.FullName, "BinkyLabs.OpenAPI.Overlays.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not find the repository root.");
    }
}