using ResultsComparer.Models;

namespace ResultsComparer.Policies;

internal interface IBenchmarkComparisonPolicy : IEqualityComparer<BenchmarkMemory>
{
    string Name { get; }
    string GetErrorMessage(BenchmarkMemory? x, BenchmarkMemory? y);

    public static IEnumerable<IBenchmarkComparisonPolicy> GetSelectedPolicies(string[] names)
    {
        if (names is [])
        {
            yield break;
        }

        var allPolicies = GetAllPolicies();
        if (names is ["all"])
        {
            foreach (var policy in allPolicies)
            {
                yield return policy;
            }

            yield break;
        }

        var indexedNames = names.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var policy in allPolicies.Where(x => indexedNames.Contains(x.Name)))
        {
            yield return policy;
        }
    }

    public static IBenchmarkComparisonPolicy[] GetAllPolicies()
    {
        return
        [
            IdenticalMemoryUsagePolicy.Instance,
            ZeroPointOnePercentDifferenceMemoryUsagePolicy.Instance,
            ZeroPointTwoPercentDifferenceMemoryUsagePolicy.Instance,
            OnePercentDifferenceMemoryUsagePolicy.Instance,
            TwoPercentDifferenceMemoryUsagePolicy.Instance,
            FivePercentDifferenceMemoryUsagePolicy.Instance,
        ];
    }
}