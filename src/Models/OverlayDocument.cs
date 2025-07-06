using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Writers;

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
    public async Task<(OpenApiDocument?, OverlayDiagnostic, OpenApiDiagnostic?)> ApplyToExtendedDocumentAsync(string format, OpenApiReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
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
    public async Task<(OpenApiDocument?, OverlayDiagnostic, OpenApiDiagnostic?)> ApplyToDocumentAsync(string documentPathOrUri, string format, OpenApiReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    { // TODO switch to the overlay reader settings when we have them
        ArgumentException.ThrowIfNullOrEmpty(documentPathOrUri);
        ArgumentNullException.ThrowIfNull(format);
        readerSettings ??= new OpenApiReaderSettings();

        // Load the document from the specified path or URI
        using var input = new MemoryStream();
        if (documentPathOrUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            documentPathOrUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            // TODO switch to the overlay reader settings http client when we have them
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(documentPathOrUri, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await response.Content.CopyToAsync(input, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            using var fileStream = new FileStream(documentPathOrUri, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(input, cancellationToken).ConfigureAwait(false);
        }
        var uri = new Uri(documentPathOrUri, UriKind.RelativeOrAbsolute);
        return await ApplyToDocumentStreamAsync(input, uri, format, readerSettings, cancellationToken).ConfigureAwait(false);
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
    public async Task<(OpenApiDocument?, OverlayDiagnostic, OpenApiDiagnostic?)> ApplyToDocumentStreamAsync(MemoryStream input, Uri location, string format, OpenApiReaderSettings? readerSettings = default, CancellationToken cancellationToken = default)
    { // TODO switch to the overlay reader settings when we have them
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(format);
        readerSettings ??= new OpenApiReaderSettings();

        JsonNode? jsonNode;

        //TODO make the format optional and implement a format detection mechanism when not provided
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
}