using System.Threading.Tasks;

namespace BinkyLabs.OpenApi.Overlays.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var app = new OverlayCliApp();
        return await app.RunAsync(args);
    }
}
