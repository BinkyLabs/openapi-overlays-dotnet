using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Writers;
using BinkyLabs.Overlay.Overlays;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;

using SharpYaml.Serialization;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents an Overlay Document as defined in the OpenAPI Overlay specification.
/// </summary>
public class OverlayDocument : IOverlaySerializable, IOverlayExtensible
{
    /// <summary>
    /// Gets or sets the overlay version. Default is "1.0.0".
    /// </summary>
    public string? Overlay { get; internal set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the overlay info object.
    /// </summary>
    public OverlayInfo? Info { get; set; }

    /// <summary>
    /// Gets or sets the value of the 'extends' property.
    /// </summary>
    public string? Extends { get; set; }

    /// <summary>
    /// Gets or sets the list of actions for the overlay.
    /// </summary>
    public IList<OverlayAction>? Actions { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }

    /// <summary>
    /// Serializes the overlay document as an OpenAPI Overlay v1.0.0 JSON object.
    /// </summary>
    /// <param name="writer">The OpenAPI writer to use for serialization.</param>
    public void SerializeAsV1(IOpenApiWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteRequiredProperty("overlay", "1.0.0");
        if (Info != null)
        {
            writer.WriteRequiredObject("info", Info, (w, obj) => obj.SerializeAsV1(w));
        }
        writer.WriteProperty("extends", Extends);
        if (Actions != null)
        {
            writer.WriteRequiredCollection<OverlayAction>("actions", Actions, (w, action) => action.SerializeAsV1(w));
        }
        writer.WriteOverlayExtensions(Extensions, OverlaySpecVersion.Overlay1_0);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads the stream input and parses it into an Open API document.
    /// </summary>
    /// <param name="stream">Stream containing OpenAPI description to parse.</param>
    /// <param name="format">The OpenAPI format to use during parsing.</param>
    /// <param name="settings">The OpenApi reader settings.</param>
    /// <returns></returns>
    public static ReadResult Load(MemoryStream stream,
                                  string? format = null,
                                  OverlayReaderSettings? settings = null)
    {
        return OverlayModelFactory.Load(stream, format, settings);
    }

    /// <summary>
    /// Parses a local file path or Url into an Open API document.
    /// </summary>
    /// <param name="url"> The path to the OpenAPI file.</param>
    /// <param name="settings">The OpenApi reader settings.</param>
    /// <param name="token">The cancellation token</param>
    /// <returns></returns>
    public static async Task<ReadResult> LoadAsync(string url, OverlayReaderSettings? settings = null, CancellationToken token = default)
    {
        return await OverlayModelFactory.LoadFormUrlAsync(url, settings, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads the stream input and parses it into an Open API document.
    /// </summary>
    /// <param name="stream">Stream containing OpenAPI description to parse.</param>
    /// <param name="format">The OpenAPI format to use during parsing.</param>
    /// <param name="settings">The OpenApi reader settings.</param>
    /// <param name="cancellationToken">Propagates information about operation cancelling.</param>
    /// <returns></returns>
    public static async Task<ReadResult> LoadAsync(Stream stream, string? format = null, OverlayReaderSettings? settings = null, CancellationToken cancellationToken = default)
    {
        return await OverlayModelFactory.LoadFromStreamAsync(stream, format, settings, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Parses a string into a <see cref="OpenApiDocument"/> object.
    /// </summary>
    /// <param name="input"> The string input.</param>
    /// <param name="format"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static ReadResult Parse(string input,
                                   string? format = null,
                                   OverlayReaderSettings? settings = null)
    {
        return OverlayModelFactory.Parse(input, format, settings);
    }

    internal bool ApplyToDocument(JsonNode jsonNode, OverlayDiagnostic overlayDiagnostic)
    {
        ArgumentNullException.ThrowIfNull(jsonNode);
        ArgumentNullException.ThrowIfNull(overlayDiagnostic);
        if (Actions is not { Count: > 0 })
        {
            return true; // No actions to apply, nothing to do
        }
        var i = 0;
        foreach (var action in Actions)
        {
            if (!action.ApplyToDocument(jsonNode, overlayDiagnostic, i))
            {
                return false; // If any action fails, the entire application fails
            }
            i++;
        }
        return true;
    }
    /// <summary>
    /// Applies the action to an OpenAPI document loaded from the extends property.
    /// The document is read in the specified format (e.g., JSON or YAML).
    /// </summary>
    /// <param name="format">The format of the document (e.g., JSON or YAML).</param>
    /// <param name="readerSettings">Settings to use when reading the document.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The OpenAPI document after applying the action.</returns>
    public async Task<(OpenApiDocument?, OverlayDiagnostic, OpenApiDiagnostic?)> ApplyToExtendedDocumentAsync(string? format = default, OpenApiReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Extends))
        {
            throw new InvalidOperationException("The 'extends' property must be set to apply the overlay to an extended document.");
        }
        return await ApplyToDocumentAsync(Extends, format, readerSettings, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the action to an OpenAPI document loaded from a specified path or URI.
    /// The document is read in the specified format (e.g., JSON or YAML).
    /// </summary>
    /// <param name="documentPathOrUri">Path or URI to the OpenAPI document.</param>
    /// <param name="format">The format of the document (e.g., JSON or YAML).</param>
    /// <param name="readerSettings">Settings to use when reading the document.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The OpenAPI document after applying the action.</returns>
    public async Task<(OpenApiDocument?, OverlayDiagnostic, OpenApiDiagnostic?)> ApplyToDocumentAsync(string documentPathOrUri, string? format = default, OpenApiReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    { // TODO switch to the overlay reader settings when we have them
        ArgumentException.ThrowIfNullOrEmpty(documentPathOrUri);
        readerSettings ??= new OpenApiReaderSettings();

        // Load the document from the specified path or URI
        Stream input;
        if (documentPathOrUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            documentPathOrUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            // TODO switch to the overlay reader settings http client when we have them
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(documentPathOrUri, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            input = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            input = new MemoryStream();
            using var fileStream = new FileStream(documentPathOrUri, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(input, cancellationToken).ConfigureAwait(false);
        }
        var uri = new Uri(documentPathOrUri, UriKind.RelativeOrAbsolute);
        var result = await ApplyToDocumentStreamAsync(input, uri, format, readerSettings, cancellationToken).ConfigureAwait(false);
        await input.DisposeAsync().ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Applies the action to an OpenAPI document loaded from a specified path or URI.
    /// The document is read in the specified format (e.g., JSON or YAML).
    /// </summary>
    /// <param name="input">A stream containing the OpenAPI document.</param>
    /// <param name="location">The URI location of the document, used for to load external references.</param>
    /// <param name="format">The format of the document (e.g., JSON or YAML).</param>
    /// <param name="readerSettings">Settings to use when reading the document.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The OpenAPI document after applying the action.</returns>
    public async Task<(OpenApiDocument?, OverlayDiagnostic, OpenApiDiagnostic?)> ApplyToDocumentStreamAsync(Stream input, Uri location, string? format = default, OpenApiReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    { // TODO switch to the overlay reader settings when we have them
        ArgumentNullException.ThrowIfNull(input);
        readerSettings ??= new OpenApiReaderSettings();

        JsonNode? jsonNode;

        if (string.IsNullOrEmpty(format))
        {
            var (bufferedStream, detectedFormat) = await PrepareStreamForReadingAsync(input, cancellationToken).ConfigureAwait(false);
            format = detectedFormat;
            input = bufferedStream;
        }
        // TODO maybe this should be a registry on the overlay reader settings?
        if (OpenApiConstants.Json.Equals(format, StringComparison.OrdinalIgnoreCase))
        {
            jsonNode = await JsonNode.ParseAsync(input, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else if (OpenApiConstants.Yaml.Equals(format, StringComparison.OrdinalIgnoreCase))
        {
            using var textReader = new StreamReader(input, System.Text.Encoding.UTF8);
            var yamlStream = new YamlStream();
            yamlStream.Load(textReader);
            jsonNode = yamlStream is { Documents.Count: > 0 }
                ? yamlStream.Documents[0].ToJsonNode()
                : throw new InvalidOperationException("No documents found in the YAML stream.");

        }
        else
        {
            throw new ArgumentException($"Unsupported format: {format}", nameof(format));
        }

        if (jsonNode is null)
        {
            throw new InvalidOperationException("Failed to parse the OpenAPI document.");
        }
        var overlayDiagnostic = new OverlayDiagnostic();
        var result = ApplyToDocument(jsonNode, overlayDiagnostic);
        if (!result)
        {
            return (null, overlayDiagnostic, null);
        }
        var openAPIJsonReader = new OpenApiJsonReader();
        var (openAPIDocument, openApiDiagnostic) = openAPIJsonReader.Read(jsonNode, location, readerSettings);
        return (openAPIDocument, overlayDiagnostic, openApiDiagnostic);
    }
    private static async Task<(Stream, string)> PrepareStreamForReadingAsync(Stream input, CancellationToken token = default)
    {
        Stream preparedStream = input;
        string format;

        if (!input.CanSeek)
        {
            // Use a temporary buffer to read a small portion for format detection
            using var bufferStream = new MemoryStream();
            await input.CopyToAsync(bufferStream, 1024, token).ConfigureAwait(false);
            bufferStream.Position = 0;

            // Inspect the format from the buffered portion
            format = InspectStreamFormat(bufferStream);

            // If format is JSON, no need to buffer further — use the original stream.
            if (format.Equals(OpenApiConstants.Json, StringComparison.OrdinalIgnoreCase))
            {
                preparedStream = input;
            }
            else
            {
                // YAML or other non-JSON format; copy remaining input to a new stream.
                preparedStream = new MemoryStream();
                bufferStream.Position = 0;
                await bufferStream.CopyToAsync(preparedStream, 81920, token).ConfigureAwait(false); // Copy buffered portion
                await input.CopyToAsync(preparedStream, 81920, token).ConfigureAwait(false); // Copy remaining data
                preparedStream.Position = 0;
            }
        }
        else
        {
            format = InspectStreamFormat(input);

            if (!format.Equals(OpenApiConstants.Json, StringComparison.OrdinalIgnoreCase))
            {
                // Buffer stream for non-JSON formats (e.g., YAML) since they require synchronous reading
                preparedStream = new MemoryStream();
                await input.CopyToAsync(preparedStream, 81920, token).ConfigureAwait(false);
                preparedStream.Position = 0;
            }
        }

        return (preparedStream, format);
    }
    private static string InspectStreamFormat(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        long initialPosition = stream.Position;
        int firstByte = stream.ReadByte();

        // Skip whitespace if present and read the next non-whitespace byte
        if (char.IsWhiteSpace((char)firstByte))
        {
            firstByte = stream.ReadByte();
        }

        stream.Position = initialPosition; // Reset the stream position to the beginning

        char firstChar = (char)firstByte;
        return firstChar switch
        {
            '{' or '[' => OpenApiConstants.Json,  // If the first character is '{' or '[', assume JSON
            _ => OpenApiConstants.Yaml             // Otherwise assume YAML
        };
    }
}