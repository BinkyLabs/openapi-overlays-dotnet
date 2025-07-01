using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

public class OverlayInfo : IOverlaySerializable
{
    public required string Title { get; set; }
    public required string Version { get; set; }

    public void SerializeAsV1(IOpenApiWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteProperty("title", Title);
        writer.WriteProperty("version", Version);
        writer.WriteEndObject();
    }
}