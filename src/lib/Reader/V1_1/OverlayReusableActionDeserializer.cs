using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

#pragma warning disable BOO002
internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableAction> ReusableActionFixedFields = new()
    {
        {
            OverlayConstants.ReusableActionDescriptionFieldName,
            (o, v, _) => o.Description = v.GetScalarValue()
        },
        {
            OverlayConstants.ReusableActionFieldsFieldName,
            (o, v, c) => o.Fields = LoadAction(v, c)
        }
    };

    public static readonly PatternFieldMap<OverlayReusableAction> ReusableActionPatternFields =
        OverlayV1Deserializer.GetActionPatternFields<OverlayReusableAction>(OverlaySpecVersion.Overlay1_1);

    public static OverlayReusableAction LoadReusableAction(JsonNode node, ParsingContext context)
    {
        var mapNode = node.CheckMapNode("ReusableAction", context);
        var action = new OverlayReusableAction();
        OverlayV1Deserializer.ParseMap(mapNode, action, ReusableActionFixedFields, ReusableActionPatternFields, context);
        return action;
    }
}
#pragma warning restore BOO002