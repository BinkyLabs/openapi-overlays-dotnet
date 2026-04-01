using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a reusable action reference action with local overrides.
/// </summary>
[Experimental("BOO002")]
public class OverlayReusableActionReference : IOverlayAction
{
    private OverlayReusableActionReferenceItem? _reference;

    /// <summary>
    /// Gets the reusable action reference data item.
    /// </summary>
    public OverlayReusableActionReferenceItem? Reference
    {
        get => _reference;
        init => _reference = value;
    }

    /// <summary>
    /// Gets the referenced target action if it has been resolved.
    /// </summary>
    public OverlayAction? TargetAction { get; internal set; }

    /// <inheritdoc/>
    public string? Target
    {
        get => Reference?.Target ?? TargetAction?.Target;
        set => EnsureReference().Target = value;
    }

    /// <inheritdoc/>
    public string? Description
    {
        get => Reference?.Description ?? TargetAction?.Description;
        set => EnsureReference().Description = value;
    }

    /// <inheritdoc/>
    public bool? Remove
    {
        get => Reference?.Remove ?? TargetAction?.Remove;
        set => EnsureReference().Remove = value;
    }

    /// <inheritdoc/>
    public JsonNode? Update
    {
        get => Reference?.Update ?? TargetAction?.Update;
        set => EnsureReference().Update = value;
    }

    /// <inheritdoc/>
    public string? Copy
    {
        get => Reference?.Copy ?? TargetAction?.Copy;
        set => EnsureReference().Copy = value;
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

    internal void SetReferenceId(string? id) => EnsureReference().Id = id;
    internal void SetReferenceParameterValues(IDictionary<string, JsonNode>? parameterValues) => EnsureReference().ParameterValues = parameterValues;

    private OverlayReusableActionReferenceItem EnsureReference()
    {
        _reference ??= new OverlayReusableActionReferenceItem();
        return _reference;
    }

    private void SerializeInternal(
        IOpenApiWriter writer,
        OverlaySpecVersion version,
        string copyFieldName)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartObject();

        if (Reference?.Reference != null)
        {
            writer.WriteProperty(OverlayConstants.ReusableActionReferenceXReferenceFieldName, Reference.Reference);
        }

        if (Reference?.ParameterValues != null)
        {
            writer.WriteOptionalMap(
                OverlayConstants.ReusableActionReferenceXParameterValuesFieldName,
                Reference.ParameterValues,
                static (w, n) => w.WriteAny(n));
        }

        if (Reference?.Target != null)
        {
            writer.WriteProperty(OverlayConstants.ActionTargetFieldName, Reference.Target);
        }

        if (Reference?.Description != null)
        {
            writer.WriteProperty(OverlayConstants.ActionDescriptionFieldName, Reference.Description);
        }

        if (Reference?.Remove.HasValue == true)
        {
            writer.WriteProperty(OverlayConstants.ActionRemoveFieldName, Reference.Remove, false);
        }

        if (Reference?.Update != null)
        {
            writer.WriteOptionalObject(OverlayConstants.ActionUpdateFieldName, Reference.Update, static (w, s) => w.WriteAny(s));
        }

        if (Reference?.Copy != null)
        {
            writer.WriteProperty(copyFieldName, Reference.Copy);
        }

        writer.WriteOverlayExtensions(Extensions, version);
        writer.WriteEndObject();
    }
}
