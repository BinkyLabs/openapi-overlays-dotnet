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
    /// Internal constructor used for deserialization
    /// </summary>
    internal OverlayReusableActionReferenceItem()
    {

    }
    /// <summary>
    /// Creates a reusable action reference item with the specified reusable action identifier and optional overlay document context for validation.
    /// </summary>
    /// <param name="reusableActionIdentifier">The identifier of the reusable action.</param>
    /// <param name="overlayDocument">The optional overlay document reference resolution.</param>
    public OverlayReusableActionReferenceItem(string reusableActionIdentifier, OverlayDocument? overlayDocument = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(reusableActionIdentifier);
        Id = reusableActionIdentifier;
        HostDocument = overlayDocument;
    }
    /// <summary>
    /// Gets the optional overlay document context for validation and reference resolution.
    /// </summary>
    public OverlayDocument? HostDocument { get; init; }
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
    public string Reference =>
        string.IsNullOrEmpty(Id) ? string.Empty : $"{OverlayConstants.ReusableActionReferencePrefix}{Id}";

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