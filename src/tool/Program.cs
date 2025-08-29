using System;
using System.Threading;
using System.Threading.Tasks;

namespace BinkyLabs.OpenApi.Overlays.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };

        return await OverlayCliApp.RunAsync(args, cts.Token);
    }
}