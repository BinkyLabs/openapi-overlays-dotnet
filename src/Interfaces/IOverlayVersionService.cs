
// Licensed under the MIT license.

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
    /// <param name="doc">A host document instance.</param>
    /// <returns>Instance of OpenAPIElement</returns>
    T? LoadElement<T>(ParseNode node, OverlayDocument doc) where T : IOpenApiElement;

    /// <summary>
    /// Converts a generic RootNode instance into a strongly typed OverlayDocument
    /// </summary>
    /// <param name="rootNode">RootNode containing the information to be converted into an OpenAPI Document</param>
    /// <param name="location">Location of where the document that is getting loaded is saved</param>
    /// <returns>Instance of OverlayDocument populated with data from rootNode</returns>
    OverlayDocument LoadDocument(RootNode rootNode, Uri location);
}