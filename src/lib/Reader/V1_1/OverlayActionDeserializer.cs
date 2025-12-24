using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayAction> ActionFixedFields = new(OverlayV1Deserializer.ActionFixedFields, ["x-copy"])
    {
        { "copy", (o, v) => o.Copy = v.GetScalarValue() },
    };
    public static readonly PatternFieldMap<OverlayAction> ActionPatternFields = OverlayV1Deserializer.GetActionPatternFields(OverlaySpecVersion.Overlay1_1);
    public static OverlayAction LoadAction(ParseNode node) => OverlayV1Deserializer.LoadActionInternal(node, ActionFixedFields, ActionPatternFields);
}