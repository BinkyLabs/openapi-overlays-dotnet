using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace performance;

public static class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance;
        BenchmarkRunner.Run<OverlayLoading>(config, args);
        BenchmarkRunner.Run<OverlayApplication>(config, args);
    }
}