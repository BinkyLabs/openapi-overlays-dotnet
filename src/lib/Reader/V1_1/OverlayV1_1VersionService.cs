
// Licensed under the MIT license.

using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

/// <summary>
/// The version service for the Overlay 1.1 specification.
/// </summary>
internal class OverlayV1_1VersionService : IOverlayVersionService
{

    /// <summary>
    /// Create Parsing Context
    /// </summary>
    /// <param name="diagnostic">Provide instance for diagnostic object for collecting and accessing information about the parsing.</param>
    public OverlayV1_1VersionService(OverlayDiagnostic diagnostic)
    {
    }

    private readonly Dictionary<Type, Func<ParseNode, object?>> _loaders = new()
    {
        [typeof(JsonNodeExtension)] = OverlayV1Deserializer.LoadAny,
        [typeof(OverlayAction)] = OverlayV1_1Deserializer.LoadAction,
        [typeof(OverlayDocument)] = OverlayV1_1Deserializer.LoadDocument,
        [typeof(OverlayInfo)] = OverlayV1_1Deserializer.LoadInfo,
    };

    public OverlayDocument LoadDocument(RootNode rootNode)
    {
        return OverlayV1_1Deserializer.LoadDocument(rootNode.GetMap());
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