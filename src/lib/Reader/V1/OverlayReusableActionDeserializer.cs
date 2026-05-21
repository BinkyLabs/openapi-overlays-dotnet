using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

#pragma warning disable BOO002
internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableAction> ReusableActionFixedFields = new()
    {
        {
            OverlayConstants.ReusableActionDescriptionFieldName,
            (o, v) => o.Description = v.GetScalarValue()
        },
        {
            OverlayConstants.ReusableActionFieldsFieldName,
            (o, v) => o.Fields = LoadAction(v)
        }
    };

    public static readonly PatternFieldMap<OverlayReusableAction> ReusableActionPatternFields =
        GetActionPatternFields<OverlayReusableAction>(OverlaySpecVersion.Overlay1_0);

    public static OverlayReusableAction LoadReusableAction(ParseNode node)
    {
        var mapNode = node.CheckMapNode("ReusableAction");
        var action = new OverlayReusableAction();
        ParseMap(mapNode, action, ReusableActionFixedFields, ReusableActionPatternFields);
        return action;
    }
}
#pragma warning restore BOO002