using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayAction> ActionFixedFields = new(
        OverlayCommonAction.GetActionFixedFields<OverlayAction>(
            OverlayConstants.ActionCopyFieldName,
            static a => a.CommonAction),
        [OverlayConstants.ActionXCopyFieldName])
    {
    };

    public static readonly PatternFieldMap<OverlayAction> ActionPatternFields =
        OverlayV1Deserializer.GetActionPatternFields<OverlayAction>(OverlaySpecVersion.Overlay1_1);

    public static OverlayAction LoadAction(ParseNode node) =>
        OverlayCommonAction.LoadActionInternal(node, ActionFixedFields, ActionPatternFields);
}