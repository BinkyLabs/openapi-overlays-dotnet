using BenchmarkDotNet.Attributes;

using BinkyLabs.OpenApi.Overlays;

using Microsoft.OpenApi;

namespace performance;

[MemoryDiagnoser]
[JsonExporter]
[ShortRunJob]
public class OverlayApplication
{
    private byte[] _descriptionInputBytes = [];
    private byte[] _descriptionOverlayBytes = [];
    private byte[] _arrayInputBytes = [];
    private byte[] _arrayOverlayBytes = [];
    private OverlayDocument _descriptionOverlay = null!;
    private OverlayDocument _arrayOverlay = null!;
    private static readonly Uri BenchmarkDocumentLocation = new("file:///benchmark.yaml");

    [GlobalSetup]
    public async Task Setup()
    {
        _descriptionInputBytes = await OverlayBenchmarkData.ReadExampleBytesAsync("description-and-summary-input.yaml").ConfigureAwait(false);
        _descriptionOverlayBytes = await OverlayBenchmarkData.ReadExampleBytesAsync("description-and-summary-overlay.yaml").ConfigureAwait(false);
        _arrayInputBytes = await OverlayBenchmarkData.ReadExampleBytesAsync("array-update-input.json").ConfigureAwait(false);
        _arrayOverlayBytes = await OverlayBenchmarkData.ReadExampleBytesAsync("array-update-overlay.json").ConfigureAwait(false);
        _descriptionOverlay = await OverlayBenchmarkData.LoadOverlayAsync(_descriptionOverlayBytes, OpenApiConstants.Yaml).ConfigureAwait(false);
        _arrayOverlay = await OverlayBenchmarkData.LoadOverlayAsync(_arrayOverlayBytes, OpenApiConstants.Json).ConfigureAwait(false);
    }

    [Benchmark]
    public async Task<OverlayApplicationResultOfJsonNode> ApplyYamlOverlayToYamlDocument()
    {
        using var stream = new MemoryStream(_descriptionInputBytes, writable: false);
        return EnsureSuccessful(await _descriptionOverlay.ApplyToDocumentStreamAsync(stream, OpenApiConstants.Yaml).ConfigureAwait(false));
    }

    [Benchmark]
    public async Task<OverlayApplicationResultOfJsonNode> ApplyJsonOverlayToJsonDocument()
    {
        using var stream = new MemoryStream(_arrayInputBytes, writable: false);
        return EnsureSuccessful(await _arrayOverlay.ApplyToDocumentStreamAsync(stream, OpenApiConstants.Json).ConfigureAwait(false));
    }

    [Benchmark]
    public async Task<OverlayApplicationResultOfOpenApiDocument> ApplyYamlOverlayAndLoadOpenApiDocument()
    {
        using var stream = new MemoryStream(_descriptionInputBytes, writable: false);
        return EnsureSuccessful(await _descriptionOverlay.ApplyToDocumentStreamAndLoadAsync(stream, BenchmarkDocumentLocation, OpenApiConstants.Yaml).ConfigureAwait(false));
    }

    private static OverlayApplicationResultOfJsonNode EnsureSuccessful(OverlayApplicationResultOfJsonNode result)
    {
        if (!result.IsSuccessful || result.Diagnostic.Errors.Count > 0)
        {
            throw new InvalidOperationException($"Failed to apply overlay: {string.Join(Environment.NewLine, result.Diagnostic.Errors.Select(static e => e.Message))}");
        }

        return result;
    }

    private static OverlayApplicationResultOfOpenApiDocument EnsureSuccessful(OverlayApplicationResultOfOpenApiDocument result)
    {
        if (!result.IsSuccessful || result.Document is null || result.Diagnostic.Errors.Count > 0 || result.OpenApiDiagnostic?.Errors.Count > 0)
        {
            var overlayErrors = result.Diagnostic.Errors.Select(static e => e.Message);
            var openApiErrors = result.OpenApiDiagnostic?.Errors.Select(static e => e.Message) ?? [];
            throw new InvalidOperationException($"Failed to apply and load overlay: {string.Join(Environment.NewLine, overlayErrors.Concat(openApiErrors))}");
        }

        return result;
    }
}