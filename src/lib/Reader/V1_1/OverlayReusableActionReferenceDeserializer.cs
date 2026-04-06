using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

#pragma warning disable BOO002
internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableActionReference> ReusableActionReferenceFixedFields = new(
        OverlayV1Deserializer.ReusableActionReferenceFixedFields,
        [OverlayConstants.ActionXCopyFieldName])
    {
        {
            OverlayConstants.ActionCopyFieldName,
            (o, v) => o.Copy = v.GetScalarValue()
        }
    };

    public static readonly PatternFieldMap<OverlayReusableActionReference> ReusableActionReferencePatternFields =
        OverlayV1Deserializer.GetActionPatternFields<OverlayReusableActionReference>(OverlaySpecVersion.Overlay1_1);

    public static OverlayReusableActionReference LoadReusableActionReference(ParseNode node) =>
        OverlayCommonAction.LoadActionInternal(node, ReusableActionReferenceFixedFields, ReusableActionReferencePatternFields);
}
#pragma warning restore BOO002