using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayAction> ActionFixedFields = new(OverlayV1Deserializer.ActionFixedFields)
    {
        { "copy", (o, v) => o.Copy = v.GetScalarValue() },
    };
    public static readonly PatternFieldMap<OverlayAction> ActionPatternFields = OverlayV1Deserializer.GetActionPatternFields(OverlaySpecVersion.Overlay1_1);
    public static OverlayAction LoadAction(ParseNode node)
    {
        var mapNode = node.CheckMapNode("Action");
        var action = new OverlayAction();
        ParseMap(mapNode, action, ActionFixedFields, ActionPatternFields);

        return action;
    }
}