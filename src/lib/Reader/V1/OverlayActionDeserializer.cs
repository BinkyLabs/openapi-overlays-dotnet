using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayAction> ActionFixedFields = new()
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
    };
    public static PatternFieldMap<OverlayAction> GetActionPatternFields(OverlaySpecVersion version) =>
    new()
    {
        {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n) => o.AddExtension(k,LoadExtension(k, n, version))}
    };
    public static readonly PatternFieldMap<OverlayAction> ActionPatternFields = GetActionPatternFields(OverlaySpecVersion.Overlay1_0);
    public static OverlayAction LoadAction(ParseNode node) => LoadActionInternal(node, ActionFixedFields, ActionPatternFields);
    public static OverlayAction LoadActionInternal(ParseNode node, FixedFieldMap<OverlayAction> actionFixedFields, PatternFieldMap<OverlayAction> actionPatternFields)
    {
        var mapNode = node.CheckMapNode("Action");
        var action = new OverlayAction();
        ParseMap(mapNode, action, actionFixedFields, actionPatternFields);

        return action;
    }
}