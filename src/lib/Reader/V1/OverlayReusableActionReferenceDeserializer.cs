using System.Text.Json.Nodes;

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
        { OverlayConstants.ReusableActionReferenceXReferenceFieldName, (o, v) => o.Reference.Id = ParseReusableActionReferenceId(v.GetScalarValue()) },
        { OverlayConstants.ReusableActionReferenceXParameterValuesFieldName, (o, v) => o.Reference.ParameterValues = LoadReusableActionReferenceParameterValues(v) },
    };

    public static readonly PatternFieldMap<OverlayReusableActionReference> ReusableActionReferencePatternFields =
        GetActionPatternFields<OverlayReusableActionReference>(OverlaySpecVersion.Overlay1_0);

    public static OverlayReusableActionReference LoadReusableActionReference(ParseNode node) =>
        OverlayCommonAction.LoadActionInternal(node, ReusableActionReferenceFixedFields, ReusableActionReferencePatternFields);

    private static string? ParseReusableActionReferenceId(string? reference)
    {
        if (string.IsNullOrEmpty(reference))
        {
            return reference;
        }

        return reference.StartsWith(OverlayConstants.ReusableActionReferencePrefix, StringComparison.Ordinal)
            ? reference[OverlayConstants.ReusableActionReferencePrefix.Length..]
            : reference;
    }

    private static IDictionary<string, JsonNode> LoadReusableActionReferenceParameterValues(ParseNode node)
    {
        if (node.CreateAny() is not JsonObject parameterValuesObject)
        {
            throw new OverlayReaderException("ReusableActionReference parameter values must be a map/object", node.Context);
        }

        return parameterValuesObject
            .Where(static kvp => kvp.Value is not null)
            .ToDictionary(static kvp => kvp.Key, static kvp => kvp.Value!);
    }
}
#pragma warning restore BOO002