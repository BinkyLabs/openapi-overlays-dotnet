using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents the serialized reusable Action Reference payload fields.
/// </summary>
[Experimental("BOO002")]
public class OverlayReusableActionReferenceItem : IOverlayExtensible
{
    /// <summary>
    /// Gets the referenced reusable action identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets the map of parameter values for the reusable action reference.
    /// </summary>
    public IDictionary<string, JsonNode>? ParameterValues { get; set; }

    /// <summary>
    /// Gets the computed reusable-action reference pointer.
    /// </summary>
    public string? Reference =>
        string.IsNullOrEmpty(Id) ? null : $"{OverlayConstants.ReusableActionReferencePrefix}{Id}";

    /// <summary>
    /// The target override.
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// The description override.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The remove override.
    /// </summary>
    public bool? Remove { get; set; }

    /// <summary>
    /// The update override.
    /// </summary>
    public JsonNode? Update { get; set; }

    /// <summary>
    /// The copy override.
    /// </summary>
    public string? Copy { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }
}