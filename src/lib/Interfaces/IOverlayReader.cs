using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;

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
    /// <param name="settings">The OpenApi reader settings.</param>  
    /// <param name="cancellationToken">Propagates notification that an operation should be canceled.</param>  
    /// <returns>A task that represents the asynchronous operation, containing the read result.</returns>  
    Task<ReadResult> ReadAsync(Stream input, OverlayReaderSettings settings, CancellationToken cancellationToken = default);

    /// <summary>  
    /// Reads the stream and returns a JsonNode representation of the input.
    /// </summary>  
    /// <param name="input">The stream input.</param>
    /// <param name="cancellationToken">Propagates notification that an operation should be canceled.</param>  
    /// <returns>A task that represents the asynchronous operation, containing the JsonNode.</returns>
    Task<JsonNode?> GetJsonNodeFromStreamAsync(Stream input, CancellationToken cancellationToken = default);
}