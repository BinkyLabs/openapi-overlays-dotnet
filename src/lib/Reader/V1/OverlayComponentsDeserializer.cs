using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

#pragma warning disable BOO002
internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayComponents> ComponentsFixedFields = new()
    {
        {
            OverlayConstants.ComponentsActionsFieldName,
            (o, v) => o.Actions = v.CreateMap<OverlayReusableAction>(n => LoadReusableAction(n))
        }
    };

    public static readonly PatternFieldMap<OverlayComponents> ComponentsPatternFields = new();

    public static OverlayComponents LoadComponents(ParseNode node) =>
        LoadComponentsInternal(node, ComponentsFixedFields, ComponentsPatternFields);

    public static OverlayComponents LoadComponentsInternal(
        ParseNode node,
        FixedFieldMap<OverlayComponents> componentsFixedFields,
        PatternFieldMap<OverlayComponents> componentsPatternFields)
    {
        var mapNode = node.CheckMapNode("Components");
        var components = new OverlayComponents();
        ParseMap(mapNode, components, componentsFixedFields, componentsPatternFields);

        return components;
    }
}
#pragma warning restore BOO002