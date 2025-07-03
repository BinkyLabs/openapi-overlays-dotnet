using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

public class OverlayInfo : IOverlaySerializable, IOpenApiExtensible
{
    public string? Title { get; set; }
    public string? Version { get; set; }
    /// <inheritdoc/>
    public IDictionary<string, IOpenApiExtension>? Extensions { get; set; }
    public void SerializeAsV1(IOpenApiWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteProperty("title", Title);
        writer.WriteProperty("version", Version);
        writer.WriteEndObject();
    }
}