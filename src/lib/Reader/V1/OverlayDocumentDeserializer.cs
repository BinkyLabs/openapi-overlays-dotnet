using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayDocument> DocumentFixedFields = new()
    {
        { OverlayConstants.DocumentOverlayFieldName, (o, v) => o.Overlay = v.GetScalarValue() },
        { OverlayConstants.DocumentExtendsFieldName, (o, v) => o.Extends = v.GetScalarValue() },
        { OverlayConstants.DocumentInfoFieldName, (o, v) => o.Info = LoadInfo(v) },
        { OverlayConstants.DocumentActionsFieldName, (o, v) => o.Actions = v.CreateList<IOverlayAction>(n => LoadActionOrReference(n)) }
    };
    public static PatternFieldMap<OverlayDocument> GetDocumentPatternFields(OverlaySpecVersion version) =>
    new()
    {
        {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n) => o.AddExtension(k, LoadExtension(k, n, version))}
    };
    public static readonly PatternFieldMap<OverlayDocument> DocumentPatternFields = GetDocumentPatternFields(OverlaySpecVersion.Overlay1_0);
    public static OverlayDocument LoadDocument(ParseNode node) => LoadDocumentInternal(node, DocumentFixedFields, DocumentPatternFields);
    public static OverlayDocument LoadDocumentInternal(ParseNode node, FixedFieldMap<OverlayDocument> documentFixedFields, PatternFieldMap<OverlayDocument> documentPatternFields)
    {
        var mapNode = node.CheckMapNode("Document");
        var document = new OverlayDocument();
        ParseMap(mapNode, document, documentFixedFields, documentPatternFields);

        return document;
    }

    private static IOverlayAction LoadActionOrReference(MapNode node)
    {
        return node[OverlayConstants.ReusableActionReferenceXReferenceFieldName] != null
            ? LoadReusableActionReference(node)
            : LoadAction(node);
    }
}