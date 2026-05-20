using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a reusable action reference action with local overrides.
/// </summary>
[Experimental("BOO002")]
public class OverlayReusableActionReference : IOverlayAction
{
    /// <summary>
    /// Creates a reusable action reference action with the specified reusable action identifier and overlay document context for validation.
    /// </summary>
    /// <param name="referenceId">The reference identifier of the reusable action.</param>
    /// <param name="hostDocument">The overlay document context for reference resolution.</param>
    /// <exception cref="ArgumentException">Thrown when the referenceId is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the hostDocument is null.</exception>
    [SetsRequiredMembers]
    public OverlayReusableActionReference(string referenceId, OverlayDocument hostDocument)
    {
        ArgumentException.ThrowIfNullOrEmpty(referenceId);
        ArgumentNullException.ThrowIfNull(hostDocument);
        Reference = new OverlayReusableActionReferenceItem
        {
            Id = referenceId,
            HostDocument = hostDocument
        };
    }
    /// <summary>
    /// Creates a reusable action reference action.
    /// </summary>
    [SetsRequiredMembers]
    public OverlayReusableActionReference()
    {
        Reference = new OverlayReusableActionReferenceItem();
    }

    /// <summary>
    /// Gets the reusable action reference data item.
    /// </summary>
    public required OverlayReusableActionReferenceItem Reference { get; init; }

    private OverlayReusableAction? targetAction;
    /// <summary>
    /// Gets the referenced target action if it has been resolved.
    /// </summary>
    public OverlayReusableAction? TargetAction
    {
        get => targetAction ??
        (!string.IsNullOrEmpty(Reference.Id) &&
        (Reference.HostDocument?.Components?.Actions?.TryGetValue(Reference.Id, out var action) ?? false) ?
            action :
            null);
        internal set => targetAction = value;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The <c>target</c> field is supplied by the reference itself; it MUST NOT be set on the
    /// referenced reusable action's <see cref="OverlayReusableAction.Fields"/>.
    /// </remarks>
    public string? Target
    {
        get => Reference.Target;
        set => Reference.Target = value;
    }

    /// <inheritdoc/>
    public string? Description
    {
        get => Reference.Description ?? TargetAction?.Fields?.Description;
        set => Reference.Description = value;
    }

    /// <inheritdoc/>
    public bool? Remove
    {
        get => Reference.Remove ?? TargetAction?.Fields?.Remove;
        set => Reference.Remove = value;
    }

    /// <inheritdoc/>
    public JsonNode? Update
    {
        get => Reference.Update ?? TargetAction?.Fields?.Update;
        set => Reference.Update = value;
    }

    /// <inheritdoc/>
    public string? Copy
    {
        get => Reference.Copy ?? TargetAction?.Fields?.Copy;
        set => Reference.Copy = value;
    }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions
    {
        get => Reference.Extensions ?? TargetAction?.Fields?.Extensions;
        set => Reference.Extensions = value;
    }

    internal OverlayAction? GetResolvedAction(OverlayDiagnostic overlayDiagnostic, int actionIndex = 0)
    {
        ArgumentNullException.ThrowIfNull(overlayDiagnostic);

        var pointer = OverlayAction.GetPointer(actionIndex);

        if (TargetAction is null)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(
                pointer,
                $"Reusable action reference '{Reference.Reference}' could not be resolved."));
            return null;
        }

        if (string.IsNullOrEmpty(Reference.Target))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(
                pointer,
                $"Reusable action reference '{Reference.Reference}' is missing the required '{OverlayConstants.ActionTargetFieldName}' field."));
            return null;
        }

        return new OverlayAction
        {
            Target = Reference.Target,
            Description = Description,
            Remove = Remove,
            Update = Update,
            Copy = Copy,
            Extensions = Extensions
        };
    }

    /// <inheritdoc/>
    public void SerializeAsV1(IOpenApiWriter writer) => SerializeInternal(
        writer,
        OverlaySpecVersion.Overlay1_0,
        OverlayConstants.ActionXCopyFieldName);

    /// <inheritdoc/>
    public void SerializeAsV1_1(IOpenApiWriter writer) => SerializeInternal(
        writer,
        OverlaySpecVersion.Overlay1_1,
        OverlayConstants.ActionCopyFieldName);

    private void SerializeInternal(
        IOpenApiWriter writer,
        OverlaySpecVersion version,
        string copyFieldName)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (string.IsNullOrEmpty(Reference.Reference))
        {
            throw new InvalidOperationException($"'{nameof(Reference.Reference)}' cannot be null or empty when serializing a reusable action reference.");
        }

        writer.WriteStartObject();

        writer.WriteProperty(OverlayConstants.ReusableActionReferenceXReferenceFieldName, Reference.Reference);

        if (!string.IsNullOrEmpty(Reference.Target))
        {
            writer.WriteProperty(OverlayConstants.ActionTargetFieldName, Reference.Target);
        }

        if (!string.IsNullOrEmpty(Reference.Description))
        {
            writer.WriteProperty(OverlayConstants.ActionDescriptionFieldName, Reference.Description);
        }

        if (Reference.Remove.HasValue)
        {
            writer.WriteProperty(OverlayConstants.ActionRemoveFieldName, Reference.Remove, false);
        }

        if (Reference.Update != null)
        {
            writer.WriteOptionalObject(OverlayConstants.ActionUpdateFieldName, Reference.Update, static (w, s) => w.WriteAny(s));
        }

        if (!string.IsNullOrEmpty(Reference.Copy))
        {
            writer.WriteProperty(copyFieldName, Reference.Copy);
        }

        writer.WriteOverlayExtensions(Reference.Extensions, version);
        writer.WriteEndObject();
    }
}