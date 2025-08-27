using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents the Info object in the OpenAPI Overlay specification.
/// </summary>
public class OverlayInfo : IOverlaySerializable, IOverlayExtensible
{
    /// <summary>
    /// Gets or sets the title of the overlay.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the version of the overlay.
    /// </summary>
    public string? Version { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }

    /// <summary>
    /// Serializes the info object as an OpenAPI Overlay v1.0.0 JSON object.
    /// </summary>
    /// <param name="writer">The OpenAPI writer to use for serialization.</param>
    public void SerializeAsV1(IOpenApiWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteProperty("title", Title);
        writer.WriteProperty("version", Version);
        writer.WriteOverlayExtensions(Extensions, OverlaySpecVersion.Overlay1_0);
        writer.WriteEndObject();
    }
}