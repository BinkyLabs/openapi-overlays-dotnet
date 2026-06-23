using System.Text.Json.Serialization;

namespace ResultsComparer.Models;

internal sealed record BenchmarkReport
{
    [JsonPropertyName("Benchmarks")]
    public Benchmark[] Benchmarks { get; init; } = [];
}

internal sealed record Benchmark
{
    [JsonPropertyName("Method")]
    public string Method { get; init; } = string.Empty;

    [JsonPropertyName("Memory")]
    public BenchmarkMemory Memory { get; init; } = new();
}

internal sealed record BenchmarkMemory
{
    [JsonPropertyName("BytesAllocatedPerOperation")]
    public long AllocatedBytes { get; init; }
}