using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

#pragma warning disable BOO002
internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableAction> ReusableActionFixedFields = new(
        OverlayV1Deserializer.ReusableActionFixedFields,
        [OverlayConstants.ActionXCopyFieldName])
    {
        {
            OverlayConstants.ActionCopyFieldName,
            (o, v) => o.Copy = v.GetScalarValue()
        }
    };

    public static readonly PatternFieldMap<OverlayReusableAction> ReusableActionPatternFields =
        OverlayV1Deserializer.GetActionPatternFields<OverlayReusableAction>(OverlaySpecVersion.Overlay1_1);

    public static OverlayReusableAction LoadReusableAction(ParseNode node) =>
        OverlayCommonAction.LoadActionInternal(node, ReusableActionFixedFields, ReusableActionPatternFields);
}
#pragma warning restore BOO002