using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>  
/// Interface for reading and parsing OpenAPI documents and fragments.  
/// </summary>  
public interface IOverlayReader
{

    /// <summary>  
    /// Async method to read the stream and parse it into an OpenAPI document.  
    /// </summary>  
    /// <param name="input">The stream input.</param>  
    /// <param name="location">Location of where the document that is getting loaded is saved.</param>  
    /// <param name="settings">The OpenApi reader settings.</param>  
    /// <param name="cancellationToken">Propagates notification that an operation should be canceled.</param>  
    /// <returns>A task that represents the asynchronous operation, containing the read result.</returns>  
    Task<ReadResult> ReadAsync(Stream input, Uri location, OverlayReaderSettings settings, CancellationToken cancellationToken = default);

    /// <summary>  
    /// Provides a synchronous method to read the input memory stream and parse it into an OpenAPI document.  
    /// </summary>  
    /// <param name="input">The memory stream input.</param>  
    /// <param name="location">Location of where the document that is getting loaded is saved.</param>  
    /// <param name="settings">The OpenApi reader settings.</param>  
    /// <returns>The result of reading the OpenAPI document.</returns>  
    ReadResult Read(MemoryStream input, Uri location, OverlayReaderSettings settings);

    /// <summary>  
    /// Reads the MemoryStream and parses the fragment of an OpenAPI description into an OpenAPI element.  
    /// </summary>  
    /// <typeparam name="T">The type of OpenAPI element to parse.</typeparam>  
    /// <param name="input">Memory stream containing OpenAPI description to parse.</param>  
    /// <param name="version">Version of the OpenAPI specification that the fragment conforms to.</param>  
    /// <param name="overlayDocument">The overlayDocument object to which the fragment belongs, used to lookup references.</param>  
    /// <param name="diagnostic">Returns diagnostic object containing errors detected during parsing.</param>  
    /// <param name="settings">The OpenApiReader settings.</param>  
    /// <returns>Instance of the newly created OpenAPI element.</returns>  
    T? ReadFragment<T>(MemoryStream input, OverlaySpecVersion version, OverlayDocument overlayDocument, out OverlayDiagnostic diagnostic, OverlayReaderSettings? settings = null) where T : IOpenApiElement;
}
