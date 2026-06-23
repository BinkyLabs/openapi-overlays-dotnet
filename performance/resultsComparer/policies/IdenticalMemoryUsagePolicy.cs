using ResultsComparer.Models;

namespace ResultsComparer.Policies;

internal sealed class IdenticalMemoryUsagePolicy : BaseBenchmarkComparisonPolicy
{
    public static IdenticalMemoryUsagePolicy Instance { get; } = new();
    protected override string TypeName => nameof(IdenticalMemoryUsagePolicy);

    public override bool Equals(BenchmarkMemory? x, BenchmarkMemory? y)
    {
        return x?.AllocatedBytes == y?.AllocatedBytes;
    }

    public override string GetErrorMessage(BenchmarkMemory? x, BenchmarkMemory? y)
    {
        return $"Allocated bytes differ: {x?.AllocatedBytes} != {y?.AllocatedBytes}";
    }
}