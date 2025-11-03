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

    /// <inheritdoc/>
    public void SerializeAsV1(IOpenApiWriter writer) => SerializeInternal(writer, OverlaySpecVersion.Overlay1_0);
    /// <inheritdoc/>
    public void SerializeAsV1_1(IOpenApiWriter writer) => SerializeInternal(writer, OverlaySpecVersion.Overlay1_1);
    private void SerializeInternal(IOpenApiWriter writer, OverlaySpecVersion version)
    {
        writer.WriteStartObject();
        writer.WriteProperty("title", Title);
        writer.WriteProperty("version", Version);
        writer.WriteOverlayExtensions(Extensions, version);
        writer.WriteEndObject();
    }
}