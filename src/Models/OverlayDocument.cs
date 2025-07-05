using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

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
}