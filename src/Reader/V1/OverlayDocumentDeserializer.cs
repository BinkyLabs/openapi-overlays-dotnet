namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayDocument> DocumentFixedFields = new()
    {
        { "overlay", (o, v, _) => o.Overlay = v.GetScalarValue() },
        { "extends", (o, v, _) => o.Extends = v.GetScalarValue() },
        { "info", (o, v, host) => o.Info = LoadInfo(v, host) },
        { "actions", (o, v, host) => o.Actions = v.CreateList<OverlayAction>(LoadAction, host) }
    };
    public static readonly PatternFieldMap<OverlayDocument> DocumentPatternFields = new()
    {
        //TODO - handle extensions
        // {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n, _) => o.AddExtension(k,LoadExtension(k, n))}
    };
    public static OverlayDocument LoadDocument(ParseNode node, OverlayDocument hostDocument)
    {
        var mapNode = node.CheckMapNode("Document");
        var info = new OverlayDocument();
        ParseMap(mapNode, info, DocumentFixedFields, DocumentPatternFields, hostDocument);

        return info;
    }
}