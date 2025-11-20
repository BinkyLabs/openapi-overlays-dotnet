// Licensed under the MIT license.

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader;

/// <summary>
/// Base class for overlay version services providing common functionality.
/// </summary>
internal abstract class BaseOverlayVersionService : IOverlayVersionService
{
    /// <summary>
    /// Dictionary of type loaders for different overlay elements.
    /// </summary>
    protected abstract Dictionary<Type, Func<ParseNode, object?>> Loaders { get; }

    /// <summary>
    /// Loads an OpenAPI Element from a document fragment
    /// </summary>
    /// <typeparam name="T">Type of element to load</typeparam>
    /// <param name="node">document fragment node</param>
    /// <returns>Instance of OpenAPIElement</returns>
    public T? LoadElement<T>(ParseNode node) where T : IOpenApiElement
    {
        if (Loaders.TryGetValue(typeof(T), out var loader) && loader(node) is T result)
        {
            return result;
        }
        return default;
    }

    /// <summary>
    /// Converts a generic RootNode instance into a strongly typed OverlayDocument
    /// </summary>
    /// <param name="rootNode">RootNode containing the information to be converted into an OpenAPI Document</param>
    /// <returns>Instance of OverlayDocument populated with data from rootNode</returns>
    public abstract OverlayDocument LoadDocument(RootNode rootNode);
}