using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi.YamlReader;

namespace BinkyLabs.OpenApi.Overlays.Tests;

/// <summary>
/// Unit tests built from examples contained in the specification.
/// </summary>
public sealed class SpecificationTests
{
    private const string SpecificationPathSegment = "Specification";
    private const string ExamplesPathSegment = "examples";
    private const string ResultsPathSegment = "results";
    private const string RECORD_MODE_ENV_VAR = "OVERLAY_TEST_RECORD_MODE";
    private const string InputSuffix = "-input";
    private const string OverlaySuffix = "-overlay";

    private static readonly Lazy<string> SpecificationsBasePath = new (() => 
    {
        var assemblyLocation = Path.GetDirectoryName(typeof(SpecificationTests).Assembly.Location);
        return Path.Combine(assemblyLocation!, SpecificationPathSegment);
    });

    public static IEnumerable<object[]> ListInputFiles()
    {
        var examplesPath = Path.Combine(SpecificationsBasePath.Value, ExamplesPathSegment);
        return Directory.GetFiles(examplesPath, $"*{InputSuffix}.*", SearchOption.TopDirectoryOnly).Select(path => new object[] { Path.GetFileName(path) });
    }
    [Fact]
    public void ListInputFilesValidation()
    {
        var inputFiles = ListInputFiles().ToList();
        Assert.NotEmpty(inputFiles);
    }

    private static bool IsRecording => Environment.GetEnvironmentVariable(RECORD_MODE_ENV_VAR) == true.ToString();

    private static async Task<JsonNode> LoadYamlDocumentAsJsonNode(string filePath)
    {
        var reader = new OverlayYamlReader();
        using var fileStream = File.OpenRead(filePath);
        return await reader.GetJsonNodeFromStreamAsync(fileStream) ?? throw new InvalidOperationException($"Failed to load YAML document as JsonNode from file: {filePath}");
    }

    private static async Task<JsonNode> GetJsonNodeForFilePath(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".json" => JsonNode.Parse(await File.ReadAllTextAsync(filePath)) ?? throw new InvalidOperationException($"Failed to parse JSON document from file: {filePath}"),
            ".yaml" or ".yml" => await LoadYamlDocumentAsJsonNode(filePath),
            _ => throw new NotSupportedException($"Unsupported file extension for input file: {filePath}")
        };
    }
    
    private static async Task WriteJsonNodeToFile(JsonNode jsonNode, string filePath, Stream stream)
	{
		switch (Path.GetExtension(filePath).ToLowerInvariant())
        {
            case ".json":
                await stream.WriteAsync(Encoding.UTF8.GetBytes(jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true })));
                break;
            case ".yaml":
            case ".yml":
                var yamlText = jsonNode.ToYamlNode();
                var yamlDocument = new SharpYaml.Serialization.YamlDocument(yamlText);
                var yamlStream = new SharpYaml.Serialization.YamlStream(yamlDocument);
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    yamlStream.Save(writer, true);
                }
                break;
            default:
                throw new NotSupportedException($"Unsupported file extension for output file: {filePath}");
        }
	}

    [Theory]
    [MemberData(nameof(ListInputFiles))]
    public async Task ValidateSpecificationExample(string inputFileName)
    {
        var overlayPath = Path.Combine(SpecificationsBasePath.Value, ExamplesPathSegment, inputFileName.Replace(InputSuffix, OverlaySuffix));
        var (overlayDocument, diags) = await OverlayDocument.LoadFromUrlAsync(overlayPath);
        Assert.NotNull(overlayDocument);
        Assert.NotNull(diags);
        Assert.Empty(diags.Errors);

        var inputFilePath = Path.Combine(SpecificationsBasePath.Value, ExamplesPathSegment, inputFileName);
        if (!File.Exists(inputFilePath))
        {
            Assert.Fail($"Input file does not exist: {inputFilePath}");
        }

        var inputJsonNode = await GetJsonNodeForFilePath(inputFilePath);
        var diagnostic = new OverlayDiagnostic();
        Assert.True(overlayDocument.ApplyToDocument(inputJsonNode, diagnostic), "Applying overlay to input document failed.");

        if (IsRecording)
        {
            var targetResultPath = Path.Combine(SpecificationsBasePath.Value, "..", "..", "..", "..", SpecificationPathSegment, ResultsPathSegment, inputFileName.Replace(InputSuffix, string.Empty, StringComparison.Ordinal));
            Directory.CreateDirectory(Path.GetDirectoryName(targetResultPath)!);
            using var fileStream = File.Create(targetResultPath);
            await WriteJsonNodeToFile(inputJsonNode, targetResultPath, fileStream);
        }
		else
        {
            var expectedResultPath = Path.Combine(SpecificationsBasePath.Value, ResultsPathSegment, inputFileName.Replace(InputSuffix, string.Empty, StringComparison.Ordinal));
            var resultFileExists = File.Exists(expectedResultPath);
            if (!IsRecording && !resultFileExists)
            {
                Assert.Fail($"Expected result file does not exist: {expectedResultPath}. To create it, set the environment variable {RECORD_MODE_ENV_VAR}=true and re-run the tests.");
            }
			var expectedJsonNode = await GetJsonNodeForFilePath(expectedResultPath);
            Assert.True(JsonNode.DeepEquals(expectedJsonNode, inputJsonNode), $"The resulting document does not match the expected result. Expected result file path: {expectedResultPath}");
		}        
	}
}