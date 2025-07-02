namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayAction> ActionFixedFields = new()
    {
        { "target", (o, v, _) => o.Target = v.GetScalarValue() },
        { "description", (o, v, _) => o.Description = v.GetScalarValue() },
        { "remove", (o, v, _) =>
            {
                if (v.GetScalarValue() is string removeValue && bool.TryParse(removeValue, out var removeBool))
                {
                    o.Remove = removeBool;
                }
            }
        }
    };
    public static readonly PatternFieldMap<OverlayAction> ActionPatternFields = new()
    {
        //TODO - handle extensions
        // {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n, _) => o.AddExtension(k,LoadExtension(k, n))}
    };
    public static OverlayAction LoadAction(ParseNode node, OverlayDocument hostDocument)
    {
        var mapNode = node.CheckMapNode("Action");
        var action = new OverlayAction();
        ParseMap(mapNode, action, ActionFixedFields, ActionPatternFields, hostDocument);

        return action;
    }
}