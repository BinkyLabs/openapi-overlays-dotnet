using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

public class OverlayInfo : IOverlaySerializable, IOverlayExtensible
{
    public string? Title { get; set; }
    public string? Version { get; set; }
    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }
    public void SerializeAsV1(IOpenApiWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteProperty("title", Title);
        writer.WriteProperty("version", Version);
        writer.WriteEndObject();
    }
}