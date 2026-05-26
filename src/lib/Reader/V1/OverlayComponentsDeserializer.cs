using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

#pragma warning disable BOO002
internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayComponents> ComponentsFixedFields = new()
    {
        {
            OverlayConstants.ComponentsActionsFieldName,
            (o, v, c) => o.Actions = v.CreateMap<OverlayReusableAction>(LoadReusableAction, c)
        }
    };

    public static readonly PatternFieldMap<OverlayComponents> ComponentsPatternFields = new();

    public static OverlayComponents LoadComponents(JsonNode node, ParsingContext context) =>
        LoadComponentsInternal(node, context, ComponentsFixedFields, ComponentsPatternFields);

    public static OverlayComponents LoadComponentsInternal(
        JsonNode node,
        ParsingContext context,
        FixedFieldMap<OverlayComponents> componentsFixedFields,
        PatternFieldMap<OverlayComponents> componentsPatternFields)
    {
        var mapNode = node.CheckMapNode("Components", context);
        var components = new OverlayComponents();
        ParseMap(mapNode, components, componentsFixedFields, componentsPatternFields, context);

        return components;
    }
}
#pragma warning restore BOO002