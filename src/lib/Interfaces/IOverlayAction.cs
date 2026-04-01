using System.Text.Json.Nodes;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents an Action Object as defined in the OpenAPI Overlay specification.
/// </summary>
public interface IOverlayAction : IOverlaySerializable, IOverlayExtensible
{
    /// <summary>
    /// REQUIRED. The target of the action (JSON Pointer or similar).
    /// </summary>
    string? Target { get; }

    /// <summary>
    /// The description of the action.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// A boolean value that indicates that the target object or array MUST be removed from the the map or array it is contained in.
    /// The default value is false.
    /// </summary>
    bool? Remove { get; }

    /// <summary>
    /// The update value to be applied to the target.
    /// </summary>
    JsonNode? Update { get; }

    /// <summary>
    /// A string value that indicates that the target object or array MUST be copied to the location indicated by this string, which MUST be a JSON Pointer.
    /// This field is mutually exclusive with the <see cref="Remove"/> and <see cref="Update"/> fields.
    /// </summary>
    string? Copy { get; }
}
