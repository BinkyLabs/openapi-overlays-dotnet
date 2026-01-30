using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using BinkyLabs.OpenApi.Overlays.Cli;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

using Xunit;

namespace BinkyLabs.OpenApi.Overlays.Cli.Tests;

public sealed class OverlayCliAppTests : IDisposable
{
    private const string jsonExtension = ".json";
    private const string yamlExtension = ".yaml";
    private readonly string _tempInputFileJson = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + jsonExtension);
    private readonly string _tempOverlayFileJson = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + jsonExtension);
    private readonly string _tempOutputFileJson = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + jsonExtension);

    private readonly string _tempInputFileYaml = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + yamlExtension);
    private readonly string _tempOverlayFileYaml = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + yamlExtension);
    private readonly string _tempOutputFileYaml = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + yamlExtension);
    
    private readonly string _tempSourceFileJson = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + jsonExtension);
    private readonly string _tempTargetFileJson = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + jsonExtension);
    private readonly string _tempGeneratedOverlayJson = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + jsonExtension);

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

    private readonly string _validOpenApiYaml =
"""
openapi: 3.1.0
info:
  title: Test API
  version: 1.0.0
  description: Test description
paths:
  /test:
    get:
      summary: Test endpoint
      responses:
        '200':
          description: OK
""";

    private readonly string _validOverlayJson =
    """
    {
        "overlay": "1.0.0",
        "info": {
            "title": "Test Overlay",
            "version": "1.0.0"
        },
        "extends": "x-extends",
        "actions": [
            {
                "target": "$.info.description",
                "description": "Remove description",
                "remove": true
            }
        ],
        "x-custom-extension": {
            "someProperty": "someValue"
        }
    }
    """;

    private readonly string _validOverlayYaml =
"""
overlay: 1.0.0
info:
  title: Test Overlay
  version: 1.0.0
extends: x-extends
actions:
  - target: $.info.description
    description: Remove description
    remove: true
x-custom-extension:
  someProperty: someValue
""";

    private readonly string _sourceOpenApiJson =
        """
        {
            "openapi": "3.1.0",
            "info": {
                "title": "Pet Store API",
                "version": "1.0.0"
            },
            "paths": {
                "/pets": {
                    "get": {
                        "summary": "List pets"
                    }
                }
            }
        }
        """;
    private readonly string _targetOpenApiJson =
        """
        {
            "openapi": "3.1.0",
            "info": {
                "title": "Pet Store API",
                "version": "2.0.0",
                "contact": {
                    "name": "Support"
                }
            },
            "paths": {
                "/pets": {
                    "get": {
                        "summary": "List all pets"
                    }
                }
            }
        }
        """;

    public OverlayCliAppTests()
    {
        File.WriteAllText(_tempInputFileJson, _validOpenApiJson); // Minimal valid OpenAPI JSON
        File.WriteAllText(_tempOverlayFileJson, _validOverlayJson); // Minimal valid overlay
        File.WriteAllText(_tempInputFileYaml, _validOpenApiYaml); // Minimal valid OpenAPI YAML
        File.WriteAllText(_tempOverlayFileYaml, _validOverlayYaml); // Minimal valid
        File.WriteAllText(_tempSourceFileJson, _sourceOpenApiJson);
        File.WriteAllText(_tempTargetFileJson, _targetOpenApiJson);
    }

    public void Dispose()
    {
        if (File.Exists(_tempInputFileJson)) File.Delete(_tempInputFileJson);
        if (File.Exists(_tempOverlayFileJson)) File.Delete(_tempOverlayFileJson);
        if (File.Exists(_tempOutputFileJson)) File.Delete(_tempOutputFileJson);
        if (File.Exists(_tempInputFileYaml)) File.Delete(_tempInputFileYaml);
        if (File.Exists(_tempOverlayFileYaml)) File.Delete(_tempOverlayFileYaml);
        if (File.Exists(_tempOutputFileYaml)) File.Delete(_tempOutputFileYaml);
        if (File.Exists(_tempSourceFileJson)) File.Delete(_tempSourceFileJson);
        if (File.Exists(_tempTargetFileJson)) File.Delete(_tempTargetFileJson);
        if (File.Exists(_tempGeneratedOverlayJson)) File.Delete(_tempGeneratedOverlayJson);
    }

    [Fact]
    public async Task RunAsync_WithArguments_ReturnsOK_Json()
    {
        var result = await OverlayCliApp.RunAsync(["apply", _tempInputFileJson, "--overlay", _tempOverlayFileJson, "-out", _tempOutputFileJson]);
        Assert.Equal(0, result);
        var (openApiDocument, diags) = await OpenApiDocument.LoadAsync(_tempOutputFileJson);
        Assert.NotNull(openApiDocument);
        Assert.NotNull(diags);
        Assert.Empty(diags.Errors);
    }

    [Fact]
    public async Task RunAsync_WithArguments_ReturnsOK_Yaml()
    {
        var result = await OverlayCliApp.RunAsync(["apply", _tempInputFileYaml, "--overlay", _tempOverlayFileYaml, "-out", _tempOutputFileYaml]);
        Assert.Equal(0, result);
        // load output file and verify content
        var openApiReaderSettings = new OpenApiReaderSettings();
        openApiReaderSettings.AddYamlReader();
        var (openApiDocument, diags) = await OpenApiDocument.LoadAsync(_tempOutputFileYaml, settings: openApiReaderSettings);
        Assert.NotNull(openApiDocument);
        Assert.NotNull(diags);
        Assert.Empty(diags.Errors);
    }

    [Fact]
    public async Task RunAsync_MissingArguments_ReturnsError()
    {
        var result = await OverlayCliApp.RunAsync([]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task RunAsync_WithForceOption_OverwritesExistingFile_Json()
    {
        // First run to create the output file
        var result1 = await OverlayCliApp.RunAsync(["apply", _tempInputFileJson, "--overlay", _tempOverlayFileJson, "-out", _tempOutputFileJson]);
        Assert.Equal(0, result1);
        Assert.True(File.Exists(_tempOutputFileJson));

        // Second run with --force to overwrite
        var result2 = await OverlayCliApp.RunAsync(["apply", _tempInputFileJson, "--overlay", _tempOverlayFileJson, "-out", _tempOutputFileJson, "--force"]);
        Assert.Equal(0, result2);
        var (openApiDocument, diags) = await OpenApiDocument.LoadAsync(_tempOutputFileJson);
        Assert.NotNull(openApiDocument);
        Assert.NotNull(diags);
        Assert.Empty(diags.Errors);
    }

    [Fact]
    public async Task RunAsync_WithForceOption_ShortForm_OverwritesExistingFile_Json()
    {
        // First run to create the output file
        var result1 = await OverlayCliApp.RunAsync(["apply", _tempInputFileJson, "--overlay", _tempOverlayFileJson, "-out", _tempOutputFileJson]);
        Assert.Equal(0, result1);
        Assert.True(File.Exists(_tempOutputFileJson));

        // Second run with -f to overwrite
        var result2 = await OverlayCliApp.RunAsync(["apply", _tempInputFileJson, "--overlay", _tempOverlayFileJson, "-out", _tempOutputFileJson, "-f"]);
        Assert.Equal(0, result2);
        var (openApiDocument, diags) = await OpenApiDocument.LoadAsync(_tempOutputFileJson);
        Assert.NotNull(openApiDocument);
        Assert.NotNull(diags);
        Assert.Empty(diags.Errors);
    }

    [Fact]
    public async Task RunAsync_WithForceOption_OverwritesExistingFile_Yaml()
    {
        // First run to create the output file
        var result1 = await OverlayCliApp.RunAsync(["apply", _tempInputFileYaml, "--overlay", _tempOverlayFileYaml, "-out", _tempOutputFileYaml]);
        Assert.Equal(0, result1);
        Assert.True(File.Exists(_tempOutputFileYaml));

        // Second run with --force to overwrite
        var result2 = await OverlayCliApp.RunAsync(["apply", _tempInputFileYaml, "--overlay", _tempOverlayFileYaml, "-out", _tempOutputFileYaml, "--force"]);
        Assert.Equal(0, result2);
        var openApiReaderSettings = new OpenApiReaderSettings();
        openApiReaderSettings.AddYamlReader();
        var (openApiDocument, diags) = await OpenApiDocument.LoadAsync(_tempOutputFileYaml, settings: openApiReaderSettings);
        Assert.NotNull(openApiDocument);
        Assert.NotNull(diags);
        Assert.Empty(diags.Errors);
    }

    [Fact]
    public async Task RunAsync_GenerateCommand_CreatesOverlayFile()
    {
        var result = await OverlayCliApp.RunAsync([
            "generate",
            _tempSourceFileJson,
            _tempTargetFileJson,
            "--output", _tempGeneratedOverlayJson,
            "--force"
        ]);
        
        Assert.Equal(0, result);
        Assert.True(File.Exists(_tempGeneratedOverlayJson));
        
        var (overlay, diags) = await OverlayDocument.LoadFromUrlAsync(_tempGeneratedOverlayJson);
        Assert.NotNull(overlay);
        Assert.NotNull(diags);
        Assert.Empty(diags.Errors);
        Assert.NotNull(overlay.Actions);
        Assert.NotEmpty(overlay.Actions);
    }

    [Fact]
    public async Task RunAsync_GenerateCommand_WithMetadata_CreatesOverlayWithInfo()
    {
        var result = await OverlayCliApp.RunAsync([
            "generate",
            _tempSourceFileJson,
            _tempTargetFileJson,
            "--output", _tempGeneratedOverlayJson,
            "--title", "Test Migration",
            "--version", "2.0.0",
            "--description", "Migration from v1 to v2",
            "--force"
        ]);
        
        Assert.Equal(0, result);
        
        var (overlay, diags) = await OverlayDocument.LoadFromUrlAsync(_tempGeneratedOverlayJson);
        Assert.NotNull(overlay);
        Assert.NotNull(overlay.Info);
        Assert.Equal("Test Migration", overlay.Info.Title);
        Assert.Equal("2.0.0", overlay.Info.Version);
        Assert.Equal("Migration from v1 to v2", overlay.Info.Description);
    }

    [Fact]
    public async Task RunAsync_GenerateCommand_WithYamlFormat_CreatesYamlFile()
    {
        var yamlOutput = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + yamlExtension);
        
        try
        {
            var result = await OverlayCliApp.RunAsync([
                "generate",
                _tempSourceFileJson,
                _tempTargetFileJson,
                "--output", yamlOutput,
                "--format", "yaml",
                "--force"
            ]);
            
            Assert.Equal(0, result);
            Assert.True(File.Exists(yamlOutput));
            
            var content = await File.ReadAllTextAsync(yamlOutput);
            Assert.Contains("overlay:", content);
            Assert.Contains("actions:", content);
        }
        finally
        {
            if (File.Exists(yamlOutput)) File.Delete(yamlOutput);
        }
    }
}