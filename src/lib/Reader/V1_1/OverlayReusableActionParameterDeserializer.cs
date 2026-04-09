using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

#pragma warning disable BOO002
internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableActionParameter> ReusableActionParameterFixedFields =
        new(OverlayV1Deserializer.ReusableActionParameterFixedFields);

    public static readonly PatternFieldMap<OverlayReusableActionParameter> ReusableActionParameterPatternFields =
        OverlayV1Deserializer.GetReusableActionParameterPatternFields(OverlaySpecVersion.Overlay1_1);

    public static OverlayReusableActionParameter LoadReusableActionParameter(ParseNode node) =>
        OverlayV1Deserializer.LoadReusableActionParameterInternal(node, ReusableActionParameterFixedFields, ReusableActionParameterPatternFields);
}
#pragma warning restore BOO002