
// Licensed under the MIT license.

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

/// <summary>
/// The version service for the Overlay 1.0 specification.
/// </summary>
internal class OverlayV1VersionService : IOverlayVersionService
{

    public OverlayV1VersionService()
    {
    }

    private static readonly Dictionary<Type, Func<ParseNode, object?>> _loaders = new()
    {
        [typeof(JsonNodeExtension)] = OverlayV1Deserializer.LoadAny,
        [typeof(OverlayAction)] = OverlayV1Deserializer.LoadAction,
        [typeof(OverlayDocument)] = OverlayV1Deserializer.LoadDocument,
        [typeof(OverlayInfo)] = OverlayV1Deserializer.LoadInfo,
    };

    public OverlayDocument LoadDocument(RootNode rootNode)
    {
        return OverlayV1Deserializer.LoadDocument(rootNode.GetMap());
    }

    public T? LoadElement<T>(ParseNode node) where T : IOpenApiElement
    {
        if (Loaders.TryGetValue(typeof(T), out var loader) && loader(node) is T result)
        {
            return result;
        }
        return default;
    }

    internal Dictionary<Type, Func<ParseNode, object?>> Loaders => _loaders;
}