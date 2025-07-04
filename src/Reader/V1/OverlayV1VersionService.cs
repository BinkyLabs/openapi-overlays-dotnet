
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

/// <summary>
/// The version service for the Overlay 1.0 specification.
/// </summary>
internal class OverlayV1VersionService : IOverlayVersionService
{

    /// <summary>
    /// Create Parsing Context
    /// </summary>
    /// <param name="diagnostic">Provide instance for diagnostic object for collecting and accessing information about the parsing.</param>
    public OverlayV1VersionService(OverlayDiagnostic diagnostic)
    {
    }

    private readonly Dictionary<Type, Func<ParseNode, OverlayDocument, object?>> _loaders = new Dictionary<Type, Func<ParseNode, OverlayDocument, object?>>
    {
        [typeof(JsonNodeExtension)] = (n, d) => OverlayV1Deserializer.LoadAny(n),
        [typeof(OverlayAction)] = OverlayV1Deserializer.LoadAction,
        [typeof(OverlayDocument)] = OverlayV1Deserializer.LoadDocument,
        [typeof(OverlayInfo)] = OverlayV1Deserializer.LoadInfo,
    };

    public OverlayDocument LoadDocument(RootNode rootNode, Uri location)
    {
        return OverlayV1Deserializer.LoadOverlayDocument(rootNode, location);
    }

    public T? LoadElement<T>(ParseNode node, OverlayDocument doc) where T : IOpenApiElement
    {
        if (Loaders.TryGetValue(typeof(T), out var loader) && loader(node, doc) is T result)
        {
            return result;
        }
        return default;
    }

    internal Dictionary<Type, Func<ParseNode, OverlayDocument, object?>> Loaders => _loaders;
}