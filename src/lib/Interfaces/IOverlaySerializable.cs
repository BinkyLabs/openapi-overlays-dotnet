using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents an OpenAPI overlay element that comes with serialization functionality.
/// </summary>
/// <remarks>
/// This interface defines methods for serializing an object to different versions of the OpenAPI Overlay specification.
/// This interface should only be implemented by this library, and not by external consumers as we will add new methods without bumping major versions which will lead to source breaking changes.
/// </remarks>
public interface IOverlaySerializable
{
    /// <summary>
    /// Serializes the object to the OpenAPI Overlay v1 format.
    /// </summary>
    /// <param name="writer">A Microsoft.OpenAPI writer</param>
    void SerializeAsV1(IOpenApiWriter writer);
    /// <summary>
    /// Serializes the object to the OpenAPI Overlay v1.1 format.
    /// </summary>
    /// <param name="writer">A Microsoft.OpenAPI writer</param>
    void SerializeAsV1_1(IOpenApiWriter writer);
}