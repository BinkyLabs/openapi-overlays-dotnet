using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    private static void ParseMap<T>(
        MapNode? mapNode,
        T domainObject,
        FixedFieldMap<T> fixedFieldMap,
        PatternFieldMap<T> patternFieldMap,
        OverlayDocument doc)
    {
        if (mapNode == null)
        {
            return;
        }

        foreach (var propertyNode in mapNode)
        {
            propertyNode.ParseField(domainObject, fixedFieldMap, patternFieldMap, doc);
        }

    }
    private static IOpenApiExtension LoadExtension(string name, ParseNode node)
    {
        if (node.Context.ExtensionParsers is not null && node.Context.ExtensionParsers.TryGetValue(name, out var parser) && parser(
            node.CreateAny(), OverlaySpecVersion.Overlay1_0) is { } result)
        {
            return result;
        }
        else
        {
            return new JsonNodeExtension(node.CreateAny());
        }
    }
}