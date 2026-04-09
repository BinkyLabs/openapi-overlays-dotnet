using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

#pragma warning disable BOO002
internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayReusableActionReference> ReusableActionReferenceFixedFields = new()
    {
        { OverlayConstants.ActionTargetFieldName, (o, v) => o.Target = v.GetScalarValue() },
        { OverlayConstants.ActionDescriptionFieldName, (o, v) => o.Description = v.GetScalarValue() },
        { OverlayConstants.ActionRemoveFieldName, (o, v) =>
            {
                if (v.GetScalarValue() is string removeValue && bool.TryParse(removeValue, out var removeBool))
                {
                    o.Remove = removeBool;
                }
            }
        },
        { OverlayConstants.ActionUpdateFieldName, (o, v) => o.Update = v.CreateAny() },
        { OverlayConstants.ActionXCopyFieldName, (o, v) => o.Copy = v.GetScalarValue() },
        { OverlayConstants.ReusableActionReferenceXReferenceFieldName, (o, v) => o.Reference.Id = OverlayReusableActionReferenceItem.NormalizeReusableActionReferenceId(v.GetScalarValue()) },
        { OverlayConstants.ReusableActionReferenceXParameterValuesFieldName, (o, v) => o.Reference.ParameterValues = LoadReusableActionReferenceParameterValues(v) },
    };

    public static readonly PatternFieldMap<OverlayReusableActionReference> ReusableActionReferencePatternFields =
        GetActionPatternFields<OverlayReusableActionReference>(OverlaySpecVersion.Overlay1_0);

    public static OverlayReusableActionReference LoadReusableActionReference(ParseNode node) =>
        OverlayCommonAction.LoadActionInternal(node, ReusableActionReferenceFixedFields, ReusableActionReferencePatternFields);

    private static IDictionary<string, string> LoadReusableActionReferenceParameterValues(ParseNode node)
    {
        var parameterValuesMap = node.CheckMapNode("ReusableActionReference parameter values");
        return parameterValuesMap.CreateSimpleMap(static valueNode => valueNode.GetScalarValue());
    }
}
#pragma warning restore BOO002