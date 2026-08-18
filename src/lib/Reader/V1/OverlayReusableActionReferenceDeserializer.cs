using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableActionReference> ReusableActionReferenceFixedFields = new()
    {
        { OverlayConstants.ActionTargetFieldName, (o, v, _) => o.Target = v.GetScalarValue() },
        { OverlayConstants.ActionDescriptionFieldName, (o, v, _) => o.Description = v.GetScalarValue() },
        { OverlayConstants.ReusableActionReferenceXReferenceFieldName, (o, v, _) => o.Reference.Id = OverlayReusableActionReferenceItem.NormalizeReusableActionReferenceId(v.GetScalarValue()) },
    };

    public static readonly PatternFieldMap<OverlayReusableActionReference> ReusableActionReferencePatternFields =
        GetActionPatternFields<OverlayReusableActionReference>(OverlaySpecVersion.Overlay1_0);

    public static OverlayReusableActionReference LoadReusableActionReference(JsonNode node, ParsingContext context)
    {
        var mapNode = node.CheckMapNode("ReusableActionReference", context);
        var action = new OverlayReusableActionReference();
        ParseMap(mapNode, action, ReusableActionReferenceFixedFields, ReusableActionReferencePatternFields, context);
        return action;
    }
}