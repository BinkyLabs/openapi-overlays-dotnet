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
}