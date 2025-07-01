using BinkyLabs.OpenApi.Overlays.Reader;

namespace BinkyLabs.OpenApi.Overlays;

internal static partial class OverlayInfoDeserializer
{
    public static readonly FixedFieldMap<OverlayInfo> InfoFixedFields = new()
    {
        { "title", (o, v, _) => o.Title = v.GetScalarValue() },
        { "version", (o, v, _) => o.Version = v.GetScalarValue() }
    };
}