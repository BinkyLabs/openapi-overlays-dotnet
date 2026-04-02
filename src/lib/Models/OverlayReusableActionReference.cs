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
    public string? Target
    {
        get => Reference.Target ?? TargetAction?.Target;
        set => Reference.Target = value;
    }

    /// <inheritdoc/>
    public string? Description
    {
        get => Reference.Description ?? TargetAction?.Description;
        set => Reference.Description = value;
    }

    /// <inheritdoc/>
    public bool? Remove
    {
        get => Reference.Remove ?? TargetAction?.Remove;
        set => Reference.Remove = value;
    }

    /// <inheritdoc/>
    public JsonNode? Update
    {
        get => Reference.Update ?? TargetAction?.Update;
        set => Reference.Update = value;
    }

    /// <inheritdoc/>
    public string? Copy
    {
        get => Reference.Copy ?? TargetAction?.Copy;
        set => Reference.Copy = value;
    }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions
    {
        get => Reference.Extensions ?? TargetAction?.Extensions;
        set => Reference.Extensions = value;
    }

    /// <summary>
    /// Validates referenced parameter values against the resolved reusable action parameters.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item>
    /// <description>Resolved parameter values (provided values plus defaults for missing optional parameters).</description>
    /// </item>
    /// <item>
    /// <description>A hash set of parameter value names that do not match any reusable action parameter definition.</description>
    /// </item>
    /// <item>
    /// <description>A hash set of reusable action parameter names that are required (no default) and missing a corresponding parameter value.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the target action is not resolved.</exception>
    public (Dictionary<string, JsonNode?> ResolvedParameterValues, HashSet<string> UndefinedParameterValues, HashSet<string> MissingRequiredParameterValues) ResolveParameterValues()
    {
        if (TargetAction is null)
        {
            throw new InvalidOperationException("Cannot resolve parameter values without a resolved target action.");
        }
        var parameterDefinitions = TargetAction.Parameters;
        var parameterValues = Reference.ParameterValues;
        var resolvedParameterValues = new Dictionary<string, JsonNode?>(StringComparer.Ordinal);
        var undefinedParameterValues = new HashSet<string>(StringComparer.Ordinal);
        var missingRequiredParameterValues = new HashSet<string>(StringComparer.Ordinal);

        if (parameterDefinitions is not { Count: > 0 })
        {
            if (parameterValues is { Count: > 0 })
            {
                foreach (var valueName in parameterValues.Keys)
                {
                    undefinedParameterValues.Add(valueName);
                }
            }

            return (resolvedParameterValues, undefinedParameterValues, missingRequiredParameterValues);
        }

        var definitionsByName = OverlayReusableActionDefinitionValidator.BuildDefinitionsByName(
            parameterDefinitions,
            "parameter");

        if (parameterValues is { Count: > 0 })
        {
            foreach (var parameterValue in parameterValues)
            {
                if (definitionsByName.ContainsKey(parameterValue.Key))
                {
                    resolvedParameterValues[parameterValue.Key] = parameterValue.Value;
                    continue;
                }

                undefinedParameterValues.Add(parameterValue.Key);
            }
        }

        foreach (var definition in definitionsByName)
        {
            var definitionName = definition.Key;
            if (resolvedParameterValues.ContainsKey(definitionName))
            {
                continue;
            }

            if (definition.Value.Default is not null)
            {
                resolvedParameterValues[definitionName] = definition.Value.Default;
                continue;
            }

            missingRequiredParameterValues.Add(definitionName);
        }

        return (resolvedParameterValues, undefinedParameterValues, missingRequiredParameterValues);
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

        writer.WriteStartObject();

        if (!string.IsNullOrEmpty(Reference.Reference))
        {
            writer.WriteProperty(OverlayConstants.ReusableActionReferenceXReferenceFieldName, Reference.Reference);
        }

        if (Reference.ParameterValues != null)
        {
            writer.WriteOptionalMap(
                OverlayConstants.ReusableActionReferenceXParameterValuesFieldName,
                Reference.ParameterValues,
                static (w, n) => w.WriteAny(n));
        }

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