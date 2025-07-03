using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

public class OverlayDocument : IOverlaySerializable, IOpenApiExtensible
{
    public string? Overlay { get; internal set; } = "1.0.0";
    public OverlayInfo? Info { get; set; }
    public string? Extends { get; set; }
    public IList<OverlayAction>? Actions { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, IOpenApiExtension>? Extensions { get; set; }

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