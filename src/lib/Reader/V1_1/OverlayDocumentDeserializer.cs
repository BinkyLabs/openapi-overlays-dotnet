using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayDocument> DocumentFixedFields = new(OverlayV1Deserializer.DocumentFixedFields)
    {
        { "info", (o, v) => o.Info = LoadInfo(v) },
        { "actions", (o, v) => o.Actions = v.CreateList<OverlayAction>(LoadAction) }
    };
    public static readonly PatternFieldMap<OverlayDocument> DocumentPatternFields = OverlayV1Deserializer.GetDocumentPatternFields(OverlaySpecVersion.Overlay1_1);
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