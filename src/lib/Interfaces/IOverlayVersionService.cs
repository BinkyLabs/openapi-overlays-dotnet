
// Licensed under the MIT license.

using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Interface to a version specific parsing implementations.
/// </summary>
internal interface IOverlayVersionService
{
    /// <summary>
    /// Loads an OpenAPI Element from a document fragment
    /// </summary>
    /// <typeparam name="T">Type of element to load</typeparam>
    /// <param name="node">document fragment node</param>
    /// <param name="context">The current parsing context.</param>
    /// <returns>Instance of OpenAPIElement</returns>
    T? LoadElement<T>(JsonNode node, ParsingContext context) where T : IOpenApiElement;

    /// <summary>
    /// Converts a generic JsonNode instance into a strongly typed OverlayDocument
    /// </summary>
    /// <param name="jsonNode">JsonNode containing the information to be converted into an OpenAPI Document</param>
    /// <param name="context">The current parsing context.</param>
    /// <returns>Instance of OverlayDocument populated with data from jsonNode</returns>
    OverlayDocument LoadDocument(JsonNode jsonNode, ParsingContext context);
}