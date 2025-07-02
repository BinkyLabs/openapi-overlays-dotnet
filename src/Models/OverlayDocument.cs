using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

public class OverlayDocument : IOverlaySerializable
{
    public string? Overlay { get; set; }
    public OverlayInfo? Info { get; set; }
    public string? Extends { get; set; }
    public IList<OverlayAction>? Actions { get; set; }

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
        writer.WriteEndObject();
    }
}