using System.Text.Json.Nodes;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    internal static void ParseMap<T>(
        JsonObject mapNode,
        T domainObject,
        FixedFieldMap<T> fixedFieldMap,
        PatternFieldMap<T> patternFieldMap,
        ParsingContext context)
    {
        mapNode.ParseMap(domainObject, fixedFieldMap, patternFieldMap, context);
    }
    public static JsonNode LoadAny(JsonNode node, ParsingContext context)
    {
        return node.CreateAny();
    }
    private static IOverlayExtension LoadExtension(string name, JsonNode node, OverlaySpecVersion version, ParsingContext context)
    {
        if (context.ExtensionParsers is not null && context.ExtensionParsers.TryGetValue(name, out var parser) && parser(
            node.CreateAny(), version) is { } result)
        {
            return result;
        }
        else
        {
            return new JsonNodeExtension(node.CreateAny());
        }
    }
}