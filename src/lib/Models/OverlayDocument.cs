using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays;
using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents an Overlay Document as defined in the OpenAPI Overlay specification.
/// </summary>
public class OverlayDocument : IOverlaySerializable, IOverlayExtensible
{
    /// <summary>
    /// Gets or sets the overlay version. Default is "1.1.0".
    /// </summary>
    public string? Overlay { get; internal set; } = "1.1.0";

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

    /// <inheritdoc/>
    public void SerializeAsV1(IOpenApiWriter writer) => SerializeInternal(writer, OverlaySpecVersion.Overlay1_0, (w, obj) => obj.SerializeAsV1(w));
    /// <inheritdoc/>
    public void SerializeAsV1_1(IOpenApiWriter writer) => SerializeInternal(writer, OverlaySpecVersion.Overlay1_1, (w, obj) => obj.SerializeAsV1_1(w));
    private void SerializeInternal(IOpenApiWriter writer, OverlaySpecVersion version, Action<IOpenApiWriter, IOverlaySerializable> serializeAction)
    {
        writer.WriteStartObject();
        writer.WriteRequiredProperty("overlay", SpecVersionToStringMap[version]);
        if (Info != null)
        {
            writer.WriteRequiredObject("info", Info, serializeAction);
        }
        writer.WriteProperty("extends", Extends);
        if (Actions != null)
        {
            writer.WriteRequiredCollection<OverlayAction>("actions", Actions, serializeAction);
        }
        writer.WriteOverlayExtensions(Extensions, version);
        writer.WriteEndObject();
    }
    private static readonly Dictionary<OverlaySpecVersion, string> SpecVersionToStringMap = new()
    {
        { OverlaySpecVersion.Overlay1_0, "1.0.0" },
        { OverlaySpecVersion.Overlay1_1, "1.1.0" },
    };

    /// <summary>
    /// Parses a local file path or Url into an Open API document.
    /// </summary>
    /// <param name="url"> The path to the OpenAPI file.</param>
    /// <param name="settings">The OpenApi reader settings.</param>
    /// <param name="token">The cancellation token</param>
    /// <returns></returns>
    public static async Task<ReadResult> LoadFromUrlAsync(string url, OverlayReaderSettings? settings = null, CancellationToken token = default)
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
    public static async Task<ReadResult> LoadFromStreamAsync(Stream stream, string? format = null, OverlayReaderSettings? settings = null, CancellationToken cancellationToken = default)
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
    public static Task<ReadResult> ParseAsync(string input,
                                   string? format = null,
                                   OverlayReaderSettings? settings = null)
    {
        return OverlayModelFactory.ParseAsync(input, format, settings);
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
        var result = true;
        foreach (var action in Actions)
        {
            if (!action.ApplyToDocument(jsonNode, overlayDiagnostic, i))
            {
                result = false; // If any action fails, the entire application fails
            }
            i++;
        }
        return result;
    }
    /// <summary>
    /// Applies the action to an OpenAPI document loaded from the extends property.
    /// The document is read in the specified format (e.g., JSON or YAML).
    /// </summary>
    /// <param name="format">The format of the document (e.g., JSON or YAML).</param>
    /// <param name="readerSettings">Settings to use when reading the document.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The OpenAPI document after applying the action.</returns>
    public async Task<OverlayApplicationResultOfJsonNode> ApplyToExtendedDocumentAsync(string? format = default, OverlayReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Extends))
        {
            throw new InvalidOperationException("The 'extends' property must be set to apply the overlay to an extended document.");
        }
        return await ApplyToDocumentAsync(Extends, format, readerSettings, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the action to an OpenAPI document loaded from the extends property.
    /// The document is read in the specified format (e.g., JSON or YAML).
    /// </summary>
    /// <param name="format">The format of the document (e.g., JSON or YAML).</param>
    /// <param name="readerSettings">Settings to use when reading the document.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The OpenAPI document after applying the action.</returns>
    public async Task<OverlayApplicationResultOfOpenApiDocument> ApplyToExtendedDocumentAndLoadAsync(string? format = default, OverlayReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Extends))
        {
            throw new InvalidOperationException("The 'extends' property must be set to apply the overlay to an extended document.");
        }
        var jsonResult = await ApplyToExtendedDocumentAsync(format, readerSettings, cancellationToken).ConfigureAwait(false);
        return LoadDocument(jsonResult, new Uri(Extends), format ?? string.Empty, readerSettings);
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
    public async Task<OverlayApplicationResultOfJsonNode> ApplyToDocumentAsync(string documentPathOrUri, string? format = default, OverlayReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(documentPathOrUri);
        readerSettings ??= new OverlayReaderSettings();

        // Load the document from the specified path or URI
        Stream input;
        if (documentPathOrUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            documentPathOrUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            using var response = await readerSettings.HttpClient.GetAsync(documentPathOrUri, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            input = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            input = new MemoryStream();
            using var fileStream = new FileStream(documentPathOrUri, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(input, cancellationToken).ConfigureAwait(false);
        }
        var result = await ApplyToDocumentStreamAsync(input, format, readerSettings, cancellationToken).ConfigureAwait(false);
        await input.DisposeAsync().ConfigureAwait(false);
        return result;
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
    public async Task<OverlayApplicationResultOfOpenApiDocument> ApplyToDocumentAndLoadAsync(string documentPathOrUri, string? format = default, OverlayReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    {
        var jsonResult = await ApplyToDocumentAsync(documentPathOrUri, format, readerSettings, cancellationToken).ConfigureAwait(false);

        // Convert file paths to absolute paths before creating URI to handle relative paths correctly
        Uri uri;
        if (documentPathOrUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            documentPathOrUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            uri = new Uri(documentPathOrUri);
        }
        else
        {
            // Convert to absolute path and then create a file URI
            var absolutePath = Path.GetFullPath(documentPathOrUri);
            uri = new Uri(absolutePath, UriKind.Absolute);
        }

        return LoadDocument(jsonResult, uri, format ?? string.Empty, readerSettings);
    }

    /// <summary>
    /// Applies the action to an OpenAPI document loaded from a specified path or URI.
    /// The document is read in the specified format (e.g., JSON or YAML).
    /// </summary>
    /// <param name="input">A stream containing the OpenAPI document.</param>
    /// <param name="format">The format of the document (e.g., JSON or YAML).</param>
    /// <param name="readerSettings">Settings to use when reading the document.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The OpenAPI document after applying the action.</returns>
    public async Task<OverlayApplicationResultOfJsonNode> ApplyToDocumentStreamAsync(Stream input, string? format = default, OverlayReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        readerSettings ??= new OverlayReaderSettings();
        if (input.CanSeek)
        {
            input.Seek(0, SeekOrigin.Begin);
        }

        if (string.IsNullOrEmpty(format))
        {
            var (bufferedStream, detectedFormat) = await PrepareStreamForReadingAsync(input, cancellationToken).ConfigureAwait(false);
            format = detectedFormat;
            input = bufferedStream;
        }
        var reader = readerSettings.GetReader(format) ?? throw new NotSupportedException($"No reader found for format '{format}'.");
        var jsonNode = await reader.GetJsonNodeFromStreamAsync(input, cancellationToken).ConfigureAwait(false) ??
            throw new InvalidOperationException("Failed to parse the OpenAPI document.");
        var overlayDiagnostic = new OverlayDiagnostic();
        var result = ApplyToDocument(jsonNode, overlayDiagnostic);
        return new OverlayApplicationResultOfJsonNode
        {
            Document = jsonNode,
            Diagnostic = overlayDiagnostic,
            IsSuccessful = result,
            OpenApiDiagnostic = new OpenApiDiagnostic()
            {
                Format = format
            }
        };
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
    public async Task<OverlayApplicationResultOfOpenApiDocument> ApplyToDocumentStreamAndLoadAsync(Stream input, Uri location, string? format = default, OverlayReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    {
        var jsonResult = await ApplyToDocumentStreamAsync(input, format, readerSettings, cancellationToken).ConfigureAwait(false);
        return LoadDocument(jsonResult, location, format ?? string.Empty, readerSettings);
    }
    internal static OverlayApplicationResultOfOpenApiDocument LoadDocument(OverlayApplicationResultOfJsonNode jsonResult, Uri location, string format, OverlayReaderSettings? readerSettings)
    {
        readerSettings ??= new OverlayReaderSettings();
        var openAPIJsonReader = new OpenApiJsonReader();
        if (jsonResult.Document is null)
        {
            return OverlayApplicationResultOfOpenApiDocument.FromJsonResultWithFailedLoad(jsonResult);
        }
        var (openAPIDocument, openApiDiagnostic) = openAPIJsonReader.Read(jsonResult.Document, location, readerSettings.OpenApiSettings);
        if (openApiDiagnostic is not null && !string.IsNullOrEmpty(format))
        {
            openApiDiagnostic.Format = format;
        }
        return OverlayApplicationResultOfOpenApiDocument.FromJsonResult(jsonResult, openAPIDocument, openApiDiagnostic);
    }
    /// <summary>
    /// Combines this overlay document with another overlay document.
    /// The returned document will be a new document, and its metadata (info, etc.) will be the one from the other document.
    /// The actions from both documents will be merged. The current document actions will be first, and the ones from the other document will be next.
    /// </summary>
    /// <param name="others"></param>
    /// <returns>The merged overlay document.</returns>
    public OverlayDocument CombineWith(params OverlayDocument[] others)
    {
        if (others is not { Length: > 0 })
        {
            throw new ArgumentException("At least one other document must be provided.", nameof(others));
        }

        var lastDocument = others[^1];
        var actions = new List<OverlayAction>(Actions ?? []);
        var mergedDocument = new OverlayDocument
        {
            Info = lastDocument.Info,
            Extensions = lastDocument.Extensions is not null
                ? new Dictionary<string, IOverlayExtension>(lastDocument.Extensions)
                : null,
            Extends = lastDocument.Extends,
            Actions = actions,
        };

        // Merge actions from all documents
        actions.AddRange(others.Where(static x => x.Actions is not null)
            .SelectMany(static x => x.Actions!));

        return mergedDocument;
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