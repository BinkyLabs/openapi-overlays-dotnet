using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayAction> ActionFixedFields = new()
    {
        { "target", (o, v) => o.Target = v.GetScalarValue() },
        { "description", (o, v) => o.Description = v.GetScalarValue() },
        { "remove", (o, v) =>
            {
                if (v.GetScalarValue() is string removeValue && bool.TryParse(removeValue, out var removeBool))
                {
                    o.Remove = removeBool;
                }
            }
        },
        { "update", (o, v) => o.Update = v.CreateAny() },
        { "x-copy", (o, v) => o.Copy = v.GetScalarValue() },
    };
    public static PatternFieldMap<OverlayAction> GetActionPatternFields(OverlaySpecVersion version) =>
    new()
    {
        {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n) => o.AddExtension(k,LoadExtension(k, n, version))}
    };
    public static readonly PatternFieldMap<OverlayAction> ActionPatternFields = GetActionPatternFields(OverlaySpecVersion.Overlay1_0);
    public static OverlayAction LoadAction(ParseNode node)
    {
        var mapNode = node.CheckMapNode("Action");
        var action = new OverlayAction();
        ParseMap(mapNode, action, ActionFixedFields, ActionPatternFields);

        return action;
    }
}