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
        return await OverlayModelFactory.LoadAsync(url, settings, token).ConfigureAwait(false);
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
        return await OverlayModelFactory.LoadAsync(stream, format, settings, cancellationToken).ConfigureAwait(false);
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
}