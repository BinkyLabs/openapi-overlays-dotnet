using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

#pragma warning disable BOO002
internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableActionParameter> ReusableActionParameterFixedFields = new()
    {
        { OverlayConstants.ReusableActionParameterNameFieldName, (o, v) => o.Name = v.GetScalarValue() },
        { OverlayConstants.ReusableActionParameterDefaultFieldName, (o, v) => o.Default = v.CreateAny() }
    };

    public static PatternFieldMap<OverlayReusableActionParameter> GetReusableActionParameterPatternFields(OverlaySpecVersion version) =>
    new()
    {
        {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n) => o.AddExtension(k, LoadExtension(k, n, version))}
    };

    public static readonly PatternFieldMap<OverlayReusableActionParameter> ReusableActionParameterPatternFields =
        GetReusableActionParameterPatternFields(OverlaySpecVersion.Overlay1_0);

    public static OverlayReusableActionParameter LoadReusableActionParameter(ParseNode node) =>
        LoadReusableActionParameterInternal(node, ReusableActionParameterFixedFields, ReusableActionParameterPatternFields);

    public static OverlayReusableActionParameter LoadReusableActionParameterInternal(
        ParseNode node,
        FixedFieldMap<OverlayReusableActionParameter> reusableActionParameterFixedFields,
        PatternFieldMap<OverlayReusableActionParameter> reusableActionParameterPatternFields)
    {
        var mapNode = node.CheckMapNode("ReusableActionParameter");
        var reusableActionParameter = new OverlayReusableActionParameter();
        ParseMap(mapNode, reusableActionParameter, reusableActionParameterFixedFields, reusableActionParameterPatternFields);

        return reusableActionParameter;
    }
}
#pragma warning restore BOO002
