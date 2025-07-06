using System.Text.Json;
using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;
using Microsoft.OpenApi.YamlReader;

using SharpYaml.Serialization;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Reader for OpenAPI overlay documents in JSON format.
/// </summary>
/// <returns></returns>
public class OverlayYamlReader : IOverlayReader
{
    private const int copyBufferSize = 4096;
    private static readonly OverlayJsonReader _jsonReader = new();
    /// <summary>
    /// Reads the memory stream input and parses it into an Open API document.
    /// </summary>
    /// <param name="input">Memory stream containing OpenAPI description to parse.</param>
    /// <param name="location">Location of where the document that is getting loaded is saved</param>
    /// <param name="settings">The Reader settings to be used during parsing.</param>
    /// <returns></returns>
    public ReadResult Read(MemoryStream input,
                           Uri location,
                           OverlayReaderSettings settings)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(settings);
        JsonNode jsonNode;

        // Parse the YAML text in the stream into a sequence of JsonNodes
        try
        {
#if NET
            // this represents net core, net5 and up
            using var stream = new StreamReader(input, default, true, -1, settings.OpenApiSettings.LeaveStreamOpen);
#else
// the implementation differs and results in a null reference exception in NETFX
            using var stream = new StreamReader(input, Encoding.UTF8, true, copyBufferSize, settings.OpenApiSettings.LeaveStreamOpen);
#endif
            jsonNode = LoadJsonNodesFromYamlDocument(stream);
        }
        catch (JsonException ex)
        {
            var diagnostic = new OverlayDiagnostic();
            diagnostic.Errors.Add(new($"#line={ex.LineNumber}", ex.Message));
            return new()
            {
                Document = null,
                Diagnostic = diagnostic
            };
        }

        return Read(jsonNode, location, settings);
    }

    /// <summary>
    /// Parses the JsonNode input into an Open API document.
    /// </summary>
    /// <param name="jsonNode">The JsonNode input.</param>
    /// <param name="location">Location of where the document that is getting loaded is saved</param>
    /// <param name="settings">The Reader settings to be used during parsing.</param>
    /// <returns></returns>
    public ReadResult Read(JsonNode jsonNode,
                           Uri location,
                           OverlayReaderSettings settings)
    {
        return _jsonReader.Read(jsonNode, location, settings);
    }

    /// <summary>
    /// Reads the stream input asynchronously and parses it into an Open API document.
    /// </summary>
    /// <param name="input">Memory stream containing OpenAPI description to parse.</param>
    /// <param name="location">Location of where the document that is getting loaded is saved</param>
    /// <param name="settings">The Reader settings to be used during parsing.</param>
    /// <param name="cancellationToken">Propagates notifications that operations should be cancelled.</param>
    /// <returns></returns>
    public async Task<ReadResult> ReadAsync(Stream input,
                                            Uri location,
                                            OverlayReaderSettings settings,
                                            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input is MemoryStream memoryStream)
        {
            return Read(memoryStream, location, settings);
        }
        else
        {
            using var preparedStream = new MemoryStream();
            await input.CopyToAsync(preparedStream, copyBufferSize, cancellationToken).ConfigureAwait(false);
            preparedStream.Position = 0;
            return Read(preparedStream, location, settings);
        }
    }

    /// <inheritdoc/>
    public T? ReadFragmentFromStream<T>(MemoryStream input,
                             OverlaySpecVersion version,
                             out OverlayDiagnostic diagnostic,
                             OverlayReaderSettings? settings = null) where T : IOpenApiElement
    {
        ArgumentNullException.ThrowIfNull(input);
        JsonNode jsonNode;

        // Parse the YAML
        try
        {
            using var stream = new StreamReader(input);
            jsonNode = LoadJsonNodesFromYamlDocument(stream);
        }
        catch (JsonException ex)
        {
            diagnostic = new();
            diagnostic.Errors.Add(new($"#line={ex.LineNumber}", ex.Message));
            return default;
        }

        return ReadFragmentFromJsonNode<T>(jsonNode, version, out diagnostic, settings);
    }

    /// <inheritdoc/>
    public T? ReadFragmentFromJsonNode<T>(JsonNode input,
     OverlaySpecVersion version,
     out OverlayDiagnostic diagnostic,
     OverlayReaderSettings? settings = null) where T : IOpenApiElement
    {
        return _jsonReader.ReadFragmentFromJsonNode<T>(input, version, out diagnostic, settings);
    }

    /// <inheritdoc/>
    public Task<JsonNode?> GetJsonNodeFromStreamAsync(Stream input, CancellationToken cancellationToken = default)
    {
        using var textReader = new StreamReader(input, System.Text.Encoding.UTF8);
        var jsonNode = LoadJsonNodesFromYamlDocument(textReader);
        return Task.FromResult<JsonNode?>(jsonNode);
    }
    static JsonNode LoadJsonNodesFromYamlDocument(TextReader input)
    {
        var yamlStream = new YamlStream();
        yamlStream.Load(input);
        return yamlStream is { Documents.Count: > 0 }
            ? yamlStream.Documents[0].ToJsonNode()
            : throw new InvalidOperationException("No documents found in the YAML stream.");
    }
}