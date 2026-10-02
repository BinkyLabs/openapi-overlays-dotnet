using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_2;

internal static partial class OverlayV1_2Deserializer
{
    public static readonly FixedFieldMap<OverlayDocument> DocumentFixedFields = new(
        OverlayV1_1Deserializer.DocumentFixedFields,
        [
            OverlayConstants.DocumentInfoFieldName,
            OverlayConstants.DocumentActionsFieldName,
            OverlayConstants.DocumentXComponentsFieldName
        ])
    {
        { OverlayConstants.DocumentInfoFieldName, (o, v, c) => o.Info = LoadInfo(v, c) },
        { OverlayConstants.DocumentActionsFieldName, (o, v, c) => o.Actions = v.CreateList<IOverlayAction>(LoadActionOrReference, c) },
        { OverlayConstants.DocumentComponentsFieldName, (o, v, c) => o.Components = LoadComponents(v, c) },
        { OverlayConstants.DocumentSelfFieldName, (o, v, c) => o.Self = OverlayV1Deserializer.LoadDocumentUri(v, c, OverlayConstants.DocumentSelfFieldName) }
    };

    public static readonly PatternFieldMap<OverlayDocument> DocumentPatternFields =
        OverlayV1Deserializer.GetDocumentPatternFields(OverlaySpecVersion.Overlay1_2);

    public static OverlayDocument LoadDocument(JsonNode node, ParsingContext context) =>
        OverlayV1Deserializer.LoadDocumentInternal(node, context, DocumentFixedFields, DocumentPatternFields);

    private static IOverlayAction LoadActionOrReference(JsonNode node, ParsingContext context)
    {
        var mapNode = node.CheckMapNode("Action", context);
        return mapNode[OverlayConstants.ReusableActionReferenceReferenceFieldName] != null
            ? LoadReusableActionReference(mapNode, context)
            : LoadAction(mapNode, context);
    }
}