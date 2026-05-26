using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

#pragma warning disable BOO002
internal static partial class OverlayV1Deserializer
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
        GetActionPatternFields<OverlayReusableAction>(OverlaySpecVersion.Overlay1_0);

    public static OverlayReusableAction LoadReusableAction(JsonNode node, ParsingContext context)
    {
        var mapNode = node.CheckMapNode("ReusableAction", context);
        var action = new OverlayReusableAction();
        ParseMap(mapNode, action, ReusableActionFixedFields, ReusableActionPatternFields, context);
        return action;
    }
}
#pragma warning restore BOO002