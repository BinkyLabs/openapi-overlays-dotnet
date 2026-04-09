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
    internal OverlayDocument? HostDocument { get; set; }
    /// <summary>
    /// Gets the referenced reusable action identifier.
    /// </summary>
    public string? Id
    {
        get;
        set => field = NormalizeReusableActionReferenceId(value);
    }

    /// <summary>
    /// Gets the map of parameter values for the reusable action reference.
    /// </summary>
    public IDictionary<string, string>? ParameterValues { get; set; }

    /// <summary>
    /// Gets the computed reusable-action reference pointer.
    /// The <see cref="Id"/> is encoded as a JSON Pointer token per RFC 6901 so that
    /// names containing <c>/</c> or <c>~</c> are represented correctly in the reference.
    /// </summary>
    public string Reference =>
        string.IsNullOrEmpty(Id) ? string.Empty : $"{OverlayConstants.ReusableActionReferencePrefix}{EncodeJsonPointerToken(Id)}";

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

    internal static string? NormalizeReusableActionReferenceId(string? referenceOrId)
    {
        if (string.IsNullOrEmpty(referenceOrId))
        {
            return referenceOrId;
        }

        return referenceOrId.StartsWith(OverlayConstants.ReusableActionReferencePrefix, StringComparison.Ordinal)
            ? DecodeJsonPointerToken(referenceOrId[OverlayConstants.ReusableActionReferencePrefix.Length..])
            : referenceOrId;
    }

    /// <summary>
    /// Encodes a string as a JSON Pointer token per RFC 6901 §3:
    /// <c>~</c> is replaced by <c>~0</c> and <c>/</c> is replaced by <c>~1</c>.
    /// </summary>
    internal static string EncodeJsonPointerToken(string token)
    {
        // Order matters: ~ must be encoded before / to avoid double-encoding.
        return token.Replace("~", "~0", StringComparison.Ordinal).Replace("/", "~1", StringComparison.Ordinal);
    }

    /// <summary>
    /// Decodes a JSON Pointer token per RFC 6901 §3:
    /// <c>~1</c> is replaced by <c>/</c> and <c>~0</c> is replaced by <c>~</c>.
    /// </summary>
    internal static string DecodeJsonPointerToken(string token)
    {
        // Order matters: ~1 must be decoded before ~0 to avoid double-decoding.
        return token.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
    }
}