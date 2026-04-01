using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a reusable Action Reference Object as defined in the OpenAPI Overlay specification.
/// See: https://spec.openapis.org/overlay/v1.2.0.html#reusable-action-reference-object
/// </summary>
[Experimental("BOO002")]
public class OverlayReusableActionReference : IOverlayAction
{
    private string? _id;
    private IDictionary<string, JsonNode>? _parametersValue;
    private string? _target;
    private string? _description;
    private bool? _remove;
    private JsonNode? _update;
    private string? _copy;

    /// <summary>
    /// Gets the referenced reusable action identifier.
    /// </summary>
    public string? Id
    {
        get => _id;
        init => _id = value;
    }

    /// <summary>
    /// Gets the map of parameter values for the reusable action reference.
    /// </summary>
    public IDictionary<string, JsonNode>? ParametersValue
    {
        get => _parametersValue;
        init => _parametersValue = value;
    }

    /// <summary>
    /// Gets the referenced target reusable action if it has been resolved.
    /// </summary>
    public OverlayAction? TargetAction { get; internal set; }

    /// <summary>
    /// Gets the computed reusable-action reference pointer.
    /// </summary>
    public string? Reference =>
        string.IsNullOrEmpty(Id) ? null : $"{OverlayConstants.ReusableActionReferencePrefix}{Id}";

    /// <inheritdoc/>
    public string? Target
    {
        get => _target ?? TargetAction?.Target;
        set => _target = value;
    }

    /// <inheritdoc/>
    public string? Description
    {
        get => _description ?? TargetAction?.Description;
        set => _description = value;
    }

    /// <inheritdoc/>
    public bool? Remove
    {
        get => _remove ?? TargetAction?.Remove;
        set => _remove = value;
    }

    /// <inheritdoc/>
    public JsonNode? Update
    {
        get => _update ?? TargetAction?.Update;
        set => _update = value;
    }

    /// <inheritdoc/>
    public string? Copy
    {
        get => _copy ?? TargetAction?.Copy;
        set => _copy = value;
    }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }

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

    internal bool IsTargetSet => _target != null;
    internal bool IsDescriptionSet => _description != null;
    internal bool IsRemoveSet => _remove.HasValue;
    internal bool IsUpdateSet => _update != null;
    internal bool IsCopySet => _copy != null;
    internal void SetId(string? id) => _id = id;
    internal void SetParametersValue(IDictionary<string, JsonNode>? parametersValue) => _parametersValue = parametersValue;

    private void SerializeInternal(
        IOpenApiWriter writer,
        OverlaySpecVersion version,
        string copyFieldName)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartObject();

        if (Reference != null)
        {
            writer.WriteProperty(OverlayConstants.ReusableActionReferenceXReferenceFieldName, Reference);
        }

        if (ParametersValue != null)
        {
            writer.WriteOptionalMap(
                OverlayConstants.ReusableActionReferenceXParameterValuesFieldName,
                ParametersValue,
                static (w, n) => w.WriteAny(n));
        }

        if (IsTargetSet)
        {
            writer.WriteProperty(OverlayConstants.ActionTargetFieldName, _target);
        }

        if (IsDescriptionSet)
        {
            writer.WriteProperty(OverlayConstants.ActionDescriptionFieldName, _description);
        }

        if (IsRemoveSet)
        {
            writer.WriteProperty(OverlayConstants.ActionRemoveFieldName, _remove, false);
        }

        if (IsUpdateSet)
        {
            writer.WriteOptionalObject(OverlayConstants.ActionUpdateFieldName, _update, static (w, s) => w.WriteAny(s));
        }

        if (IsCopySet)
        {
            writer.WriteProperty(copyFieldName, _copy);
        }

        writer.WriteOverlayExtensions(Extensions, version);
        writer.WriteEndObject();
    }
}