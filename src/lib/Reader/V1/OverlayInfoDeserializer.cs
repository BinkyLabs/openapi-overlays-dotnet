using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayInfo> InfoFixedFields = new()
    {
        { "title", (o, v) => o.Title = v.GetScalarValue() },
        { "version", (o, v) => o.Version = v.GetScalarValue() }
    };
    public static readonly PatternFieldMap<OverlayInfo> InfoPatternFields = new()
    {
        {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n) => o.AddExtension(k,LoadExtension(k, n))}
    };
    public static OverlayInfo LoadInfo(ParseNode node)
    {
        var mapNode = node.CheckMapNode("Info");
        var info = new OverlayInfo();
        ParseMap(mapNode, info, InfoFixedFields, InfoPatternFields);

        return info;
    }
}