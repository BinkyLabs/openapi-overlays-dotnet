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
    private readonly string _tempInputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
    private readonly string _tempOverlayFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
    private readonly string _tempOutputFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    private readonly string _validOpenApiJson =
        """
        {
            "openapi": "3.1.0",
            "info": {
                "title": "Test API",
                "version": "1.0.0",
                "description": "Test description"
            },
            "paths": {
                "/test": {
                    "get": {
                        "summary": "Test endpoint",
                        "responses": {
                            "200": {
                                "description": "OK"
                            }
                        }
                    }
                }
            }
        }
        """;

    private readonly string _validOverlayJson = @"
    {
        ""overlay"": ""1.0.0"",
        ""info"": {
            ""title"": ""Test Overlay"",
            ""version"": ""1.0.0""
        },
        ""extends"": ""x-extends"",
        ""actions"": [
            {
                ""target"": ""$.info.description"",
                ""description"": ""Remove description"",
                ""remove"": true
            }
        ],
        ""x-custom-extension"": {
            ""someProperty"": ""someValue""
        }
    }";

    public OverlayCliAppTests()
    {
        File.WriteAllText(_tempInputFile, _validOpenApiJson); // Minimal valid OpenAPI JSON
        File.WriteAllText(_tempOverlayFile, _validOverlayJson); // Minimal valid overlay
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
        var result = await OverlayCliApp.RunAsync(["apply", _tempInputFile, "--overlay", _tempOverlayFile, "-out", _tempOutputFile]);
        Assert.Equal(0, result);
    }


    [Fact]
    public async Task RunAsync_MissingArguments_ReturnsError()
    {
        var result = await OverlayCliApp.RunAsync([]);
        Assert.Equal(1, result);
    }
}