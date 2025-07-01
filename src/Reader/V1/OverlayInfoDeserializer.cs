namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayInfo> InfoFixedFields = new()
    {
        { "title", (o, v, _) => o.Title = v.GetScalarValue() },
        { "version", (o, v, _) => o.Version = v.GetScalarValue() }
    };
    public static readonly PatternFieldMap<OverlayInfo> InfoPatternFields = new()
    {
        //TODO - handle extensions
        // {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n, _) => o.AddExtension(k,LoadExtension(k, n))}
    };
    public static OverlayInfo LoadInfo(ParseNode node, OverlayDocument hostDocument)
    {
        var mapNode = node.CheckMapNode("Info");
        var info = new OverlayInfo();
        ParseMap(mapNode, info, InfoFixedFields, InfoPatternFields, hostDocument);

        return info;
    }
}