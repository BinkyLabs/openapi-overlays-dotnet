using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

internal static partial class OverlayV1_1Deserializer
{
    private static void ParseMap<T>(
        MapNode? mapNode,
        T domainObject,
        FixedFieldMap<T> fixedFieldMap,
        PatternFieldMap<T> patternFieldMap) => OverlayV1Deserializer.ParseMap(mapNode, domainObject, fixedFieldMap, patternFieldMap);
}