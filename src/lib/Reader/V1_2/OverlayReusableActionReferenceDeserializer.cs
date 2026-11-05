using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_2;

internal static partial class OverlayV1_2Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableActionReference> ReusableActionReferenceFixedFields = new(
        OverlayV1_1Deserializer.ReusableActionReferenceFixedFields,
        [OverlayConstants.ReusableActionReferenceXReferenceFieldName])
    {
        {
            OverlayConstants.ReusableActionReferenceReferenceFieldName,
            (o, v, _) => o.Reference.Id = OverlayReusableActionReferenceItem.NormalizeReusableActionReferenceId(v.GetScalarValue())
        }
    };

    public static readonly PatternFieldMap<OverlayReusableActionReference> ReusableActionReferencePatternFields =
        OverlayV1Deserializer.GetActionPatternFields<OverlayReusableActionReference>(OverlaySpecVersion.Overlay1_2);

    public static OverlayReusableActionReference LoadReusableActionReference(JsonNode node, ParsingContext context)
    {
        var mapNode = node.CheckMapNode("ReusableActionReference", context);
        var action = new OverlayReusableActionReference();
        OverlayV1Deserializer.ParseMap(mapNode, action, ReusableActionReferenceFixedFields, ReusableActionReferencePatternFields, context);
        return action;
    }
}