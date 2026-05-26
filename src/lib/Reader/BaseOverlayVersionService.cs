// Licensed under the MIT license.

using System.Text.Json.Nodes;

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
    protected abstract Dictionary<Type, Func<JsonNode, ParsingContext, object?>> Loaders { get; }

    /// <summary>
    /// Loads an OpenAPI Element from a document fragment
    /// </summary>
    /// <typeparam name="T">Type of element to load</typeparam>
    /// <param name="node">document fragment node</param>
    /// <param name="context">The current parsing context.</param>
    /// <returns>Instance of OpenAPIElement</returns>
    public T? LoadElement<T>(JsonNode node, ParsingContext context) where T : IOpenApiElement
    {
        if (Loaders.TryGetValue(typeof(T), out var loader) && loader(node, context) is T result)
        {
            return result;
        }
        return default;
    }

    /// <summary>
    /// Converts a generic JsonNode instance into a strongly typed OverlayDocument
    /// </summary>
    /// <param name="jsonNode">JsonNode containing the information to be converted into an OpenAPI Document</param>
    /// <param name="context">The current parsing context.</param>
    /// <returns>Instance of OverlayDocument populated with data from jsonNode</returns>
    public abstract OverlayDocument LoadDocument(JsonNode jsonNode, ParsingContext context);
}