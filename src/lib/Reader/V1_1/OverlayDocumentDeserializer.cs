using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

#pragma warning disable BOO002
internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayDocument> DocumentFixedFields = new(OverlayV1Deserializer.DocumentFixedFields, [OverlayConstants.DocumentInfoFieldName, OverlayConstants.DocumentActionsFieldName, OverlayConstants.DocumentXComponentsFieldName])
    {
        { OverlayConstants.DocumentInfoFieldName, (o, v) => o.Info = LoadInfo(v) },
        { OverlayConstants.DocumentActionsFieldName, (o, v) => o.Actions = v.CreateList<IOverlayAction>(n => LoadActionOrReference(n)) },
        { OverlayConstants.DocumentXComponentsFieldName, (o, v) => o.Components = LoadComponents(v) }
    };
    public static readonly PatternFieldMap<OverlayDocument> DocumentPatternFields = OverlayV1Deserializer.GetDocumentPatternFields(OverlaySpecVersion.Overlay1_1);
    public static OverlayDocument LoadDocument(ParseNode node) => OverlayV1Deserializer.LoadDocumentInternal(node, DocumentFixedFields, DocumentPatternFields);

    private static IOverlayAction LoadActionOrReference(MapNode node)
    {
        return node[OverlayConstants.ReusableActionReferenceXReferenceFieldName] != null
            ? LoadReusableActionReference(node)
            : LoadAction(node);
    }
}
#pragma warning restore BOO002