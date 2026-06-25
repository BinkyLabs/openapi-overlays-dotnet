using BenchmarkDotNet.Attributes;

using BinkyLabs.OpenApi.Overlays;

using Microsoft.OpenApi;

namespace performance;

[MemoryDiagnoser]
[JsonExporter]
[ShortRunJob]
public class OverlayLoading
{
    private byte[] _yamlOverlayBytes = [];
    private byte[] _jsonOverlayBytes = [];

    [GlobalSetup]
    public async Task Setup()
    {
        _yamlOverlayBytes = await OverlayBenchmarkData.ReadExampleBytesAsync("traits-overlay.yaml").ConfigureAwait(false);
        _jsonOverlayBytes = await OverlayBenchmarkData.ReadExampleBytesAsync("array-update-overlay.json").ConfigureAwait(false);
    }

    [Benchmark]
    public async Task<ReadResult> LoadYamlOverlay()
    {
        using var stream = new MemoryStream(_yamlOverlayBytes, writable: false);
        return EnsureLoaded(await OverlayDocument.LoadFromStreamAsync(stream, OpenApiConstants.Yaml).ConfigureAwait(false));
    }

    [Benchmark]
    public async Task<ReadResult> LoadJsonOverlay()
    {
        using var stream = new MemoryStream(_jsonOverlayBytes, writable: false);
        return EnsureLoaded(await OverlayDocument.LoadFromStreamAsync(stream, OpenApiConstants.Json).ConfigureAwait(false));
    }

    private static ReadResult EnsureLoaded(ReadResult result)
    {
        if (result.Document is null || result.Diagnostic?.Errors.Count > 0)
        {
            throw new InvalidOperationException($"Failed to load overlay: {string.Join(Environment.NewLine, result.Diagnostic?.Errors.Select(static e => e.Message) ?? [])}");
        }

        return result;
    }
}