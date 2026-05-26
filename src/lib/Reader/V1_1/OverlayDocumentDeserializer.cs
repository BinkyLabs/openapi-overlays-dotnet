using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

#pragma warning disable BOO002
internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayDocument> DocumentFixedFields = new(OverlayV1Deserializer.DocumentFixedFields, [OverlayConstants.DocumentInfoFieldName, OverlayConstants.DocumentActionsFieldName, OverlayConstants.DocumentXComponentsFieldName])
    {
        { OverlayConstants.DocumentInfoFieldName, (o, v, c) => o.Info = LoadInfo(v, c) },
        { OverlayConstants.DocumentActionsFieldName, (o, v, c) => o.Actions = v.CreateList<IOverlayAction>(LoadActionOrReference, c) },
        { OverlayConstants.DocumentXComponentsFieldName, (o, v, c) => o.Components = LoadComponents(v, c) }
    };
    public static readonly PatternFieldMap<OverlayDocument> DocumentPatternFields = OverlayV1Deserializer.GetDocumentPatternFields(OverlaySpecVersion.Overlay1_1);
    public static OverlayDocument LoadDocument(JsonNode node, ParsingContext context) => OverlayV1Deserializer.LoadDocumentInternal(node, context, DocumentFixedFields, DocumentPatternFields);

    private static IOverlayAction LoadActionOrReference(JsonNode node, ParsingContext context)
    {
        var mapNode = node.CheckMapNode("Action", context);
        return mapNode[OverlayConstants.ReusableActionReferenceXReferenceFieldName] != null
            ? LoadReusableActionReference(mapNode, context)
            : LoadAction(mapNode, context);
    }
}
#pragma warning restore BOO002