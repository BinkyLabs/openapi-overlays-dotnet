using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayDocument> DocumentFixedFields = new()
    {
        { "overlay", (o, v) => o.Overlay = v.GetScalarValue() },
        { "extends", (o, v) => o.Extends = v.GetScalarValue() },
        { "info", (o, v) => o.Info = LoadInfo(v) },
        { "actions", (o, v) => o.Actions = v.CreateList<OverlayAction>(LoadAction) }
    };
    public static PatternFieldMap<OverlayDocument> GetDocumentPatternFields(OverlaySpecVersion version)
    {
        return new PatternFieldMap<OverlayDocument>()
        {
            {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n) => o.AddExtension(k, LoadExtension(k, n, version))}
        };
    }
    public static readonly PatternFieldMap<OverlayDocument> DocumentPatternFields = GetDocumentPatternFields(OverlaySpecVersion.Overlay1_0);
    public static OverlayDocument LoadOverlayDocument(RootNode rootNode, Uri location)
    {
        var document = new OverlayDocument();
        ParseMap(rootNode.GetMap(), document, DocumentFixedFields, DocumentPatternFields);
        return document;
    }
    public static OverlayDocument LoadDocument(ParseNode node)
    {
        var mapNode = node.CheckMapNode("Document");
        var info = new OverlayDocument();
        ParseMap(mapNode, info, DocumentFixedFields, DocumentPatternFields);

        return info;
    }
}