using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

public class OverlayInfo : IOverlaySerializable
{
    public string? Title { get; set; }
    public string? Version { get; set; }

    public void SerializeAsV1(IOpenApiWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteProperty("title", Title);
        writer.WriteProperty("version", Version);
        writer.WriteEndObject();
    }
}