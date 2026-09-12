using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_2;

internal static partial class OverlayV1_2Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableAction> ReusableActionFixedFields = new(
        OverlayV1_1Deserializer.ReusableActionFixedFields,
        [OverlayConstants.ReusableActionFieldsFieldName])
    {
        { OverlayConstants.ReusableActionFieldsFieldName, (o, v, c) => o.Fields = LoadAction(v, c) }
    };

    public static readonly PatternFieldMap<OverlayReusableAction> ReusableActionPatternFields =
        OverlayV1Deserializer.GetActionPatternFields<OverlayReusableAction>(OverlaySpecVersion.Overlay1_2);

    public static OverlayReusableAction LoadReusableAction(JsonNode node, ParsingContext context)
    {
        var mapNode = node.CheckMapNode("ReusableAction", context);
        var action = new OverlayReusableAction();
        OverlayV1Deserializer.ParseMap(mapNode, action, ReusableActionFixedFields, ReusableActionPatternFields, context);
        return action;
    }
}