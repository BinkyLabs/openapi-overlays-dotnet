using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents an OpenAPI overlay element that comes with serialization functionality.
/// </summary>
public interface IOverlaySerializable
{
    /// <summary>
    /// Serializes the object to the OpenAPI Overlay v1 format.
    /// </summary>
    /// <param name="writer">A Microsoft.OpenAPI writer</param>
    void SerializeAsV1(IOpenApiWriter writer);
}