using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;
/// <summary>
/// Represents an Action Object as defined in the OpenAPI Overlay specification v1.0.0.
/// See: https://spec.openapis.org/overlay/v1.0.0.html#action-object
/// </summary>
public class OverlayAction : IOverlaySerializable, IOverlayExtensible
{
    /// <summary>
    /// REQUIRED. The target of the action (JSON Pointer or similar).
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// The description of the action.
    /// </summary>
    public string? Description { get; set; }


    /// <summary>
    /// A boolean value that indicates that the target object or array MUST be removed from the the map or array it is contained in.
    /// The default value is false.
    /// </summary>
    public bool? Remove { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }

    // create serializer method like OverleyInfo.SerializeAsV1
    public void SerializeAsV1(IOpenApiWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteRequiredProperty("target", Target);
        writer.WriteProperty("description", Description);
        writer.WriteProperty("remove", Remove, false);
        writer.WriteEndObject();
    }
}