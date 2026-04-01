using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

#pragma warning disable BOO002
internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableAction> ReusableActionFixedFields = new(
        OverlayCommonAction.GetActionFixedFields<OverlayReusableAction>(
            OverlayConstants.ActionXCopyFieldName,
            static a => a.CommonAction))
    {
        {
            OverlayConstants.ReusableActionParametersFieldName,
            (o, v) => o.Parameters = v.CreateList<OverlayReusableActionParameter>(n => LoadReusableActionParameter(n))
        },
        {
            OverlayConstants.ReusableActionEnvironmentVariablesFieldName,
            (o, v) => o.EnvironmentVariables = v.CreateList<OverlayReusableActionParameter>(n => LoadReusableActionParameter(n))
        }
    };

    public static readonly PatternFieldMap<OverlayReusableAction> ReusableActionPatternFields =
        GetActionPatternFields<OverlayReusableAction>(OverlaySpecVersion.Overlay1_0);

    public static OverlayReusableAction LoadReusableAction(ParseNode node) =>
        OverlayCommonAction.LoadActionInternal(node, ReusableActionFixedFields, ReusableActionPatternFields);
}
#pragma warning restore BOO002