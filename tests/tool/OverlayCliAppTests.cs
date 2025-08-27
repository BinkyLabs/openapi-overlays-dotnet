using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BinkyLabs.OpenApi.Overlays.Cli;
using System.Reflection;
using Xunit;

namespace BinkyLabs.OpenApi.Overlays.Cli.Tests;

public sealed class OverlayCliAppTests : IDisposable
{
    private readonly string _tempInputFile = Path.GetTempFileName();
    private readonly string _tempOverlayFile = Path.GetTempFileName();
    private readonly string _tempOutputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".out");

    public OverlayCliAppTests()
    {
        File.WriteAllText(_tempInputFile, "{}"); // Minimal valid OpenAPI JSON
        File.WriteAllText(_tempOverlayFile, "{}"); // Minimal valid overlay
    }

    public void Dispose()
    {
        if (File.Exists(_tempInputFile)) File.Delete(_tempInputFile);
        if (File.Exists(_tempOverlayFile)) File.Delete(_tempOverlayFile);
        if (File.Exists(_tempOutputFile)) File.Delete(_tempOutputFile);
    }

    [Fact]
    public async Task RunAsync_WithArguments_ReturnsOK()
    {
        var app = new OverlayCliApp();
        var result = await app.RunAsync([_tempInputFile, "--overlay", _tempOverlayFile]);
        Assert.Equal(0, result);
    }


    [Fact]
    public async Task RunAsync_MissingArguments_ReturnsError()
    {
        var app = new OverlayCliApp();
        var result = await app.RunAsync([]);
        Assert.Equal(1, result);
    }

    [Fact]
    public void InspectStreamFormat_JsonAndYamlDetection()
    {
        var app = new OverlayCliApp();

        // JSON stream
        using var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{"));
        var jsonFormat = typeof(OverlayCliApp).GetMethod("InspectStreamFormat", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(jsonFormat);
        var resultJson = Assert.IsType<string>(jsonFormat.Invoke(app, new object[] { jsonStream }));
        Assert.Equal("json", resultJson);

        // YAML stream
        using var yamlStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("openapi: 3.0.0"));
        var resultYaml = Assert.IsType<string>(jsonFormat.Invoke(app, new object[] { yamlStream }));
        Assert.Equal("yaml", resultYaml);
    }
}