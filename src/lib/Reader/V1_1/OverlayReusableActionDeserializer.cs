using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

#pragma warning disable BOO002
internal static partial class OverlayV1_1Deserializer
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
        OverlayV1Deserializer.GetActionPatternFields<OverlayReusableAction>(OverlaySpecVersion.Overlay1_1);

    public static OverlayReusableAction LoadReusableAction(ParseNode node)
    {
        var mapNode = node.CheckMapNode("ReusableAction");
        var action = new OverlayReusableAction();
        OverlayV1Deserializer.ParseMap(mapNode, action, ReusableActionFixedFields, ReusableActionPatternFields);
        return action;
    }
}
#pragma warning restore BOO002