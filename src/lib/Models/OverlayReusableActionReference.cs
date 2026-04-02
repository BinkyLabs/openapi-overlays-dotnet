using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a reusable action reference action with local overrides.
/// </summary>
[Experimental("BOO002")]
public partial class OverlayReusableActionReference : IOverlayAction
{
    private static Regex PlaceholderPattern => PlaceholderRegex();

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

    internal OverlayAction? GetResolvedAction(OverlayDiagnostic overlayDiagnostic, IDictionary<string, string> environmentVariableValues)
    {
        ArgumentNullException.ThrowIfNull(overlayDiagnostic);
        ArgumentNullException.ThrowIfNull(environmentVariableValues);

        var pointer = Reference.Reference;
        Dictionary<string, JsonNode?> resolvedParameterValues;
        Dictionary<string, JsonNode?> resolvedEnvironmentVariableValues;
        try
        {
            var (resolvedParameters, undefinedParameterValues, missingRequiredParameterValues) = ResolveParameterValues();
            resolvedParameterValues = resolvedParameters;
            var hasErrors = false;

            if (undefinedParameterValues.Count > 0)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(
                    pointer,
                    $"Reusable action reference contains undefined parameter values: {GetOrderedNames(undefinedParameterValues)}."));
                hasErrors = true;
            }

            if (missingRequiredParameterValues.Count > 0)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(
                    pointer,
                    $"Reusable action reference is missing required parameter values: {GetOrderedNames(missingRequiredParameterValues)}."));
                hasErrors = true;
            }

            var (resolvedEnvironmentVariables, missingRequiredEnvironmentVariableValues) = TargetAction!.ResolveEnvironmentVariableValues(environmentVariableValues);
            resolvedEnvironmentVariableValues = resolvedEnvironmentVariables;
            if (missingRequiredEnvironmentVariableValues.Count > 0)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(
                    pointer,
                    $"Reusable action reference is missing required environment variable values: {GetOrderedNames(missingRequiredEnvironmentVariableValues)}."));
                hasErrors = true;
            }

            if (hasErrors)
            {
                return null;
            }
        }
        catch (InvalidOperationException ex)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(pointer, ex.Message));
            return null;
        }

        var resolvedTarget = ResolveActionStringProperty(
            Target,
            Reference.Target,
            OverlayConstants.ActionTargetFieldName,
            pointer,
            overlayDiagnostic,
            resolvedEnvironmentVariableValues,
            resolvedParameterValues);
        var resolvedDescription = ResolveActionStringProperty(
            Description,
            Reference.Description,
            OverlayConstants.ActionDescriptionFieldName,
            pointer,
            overlayDiagnostic,
            resolvedEnvironmentVariableValues,
            resolvedParameterValues);
        var resolvedCopy = ResolveActionStringProperty(
            Copy,
            Reference.Copy,
            OverlayConstants.ActionCopyFieldName,
            pointer,
            overlayDiagnostic,
            resolvedEnvironmentVariableValues,
            resolvedParameterValues);

        return new OverlayAction
        {
            Target = resolvedTarget,
            Description = resolvedDescription,
            Remove = Remove,
            Update = Update,
            Copy = resolvedCopy,
            Extensions = Extensions
        };
    }

    internal string ReplaceValues(
        string value,
        IDictionary<string, JsonNode?> resolvedEnvironmentVariableValues,
        IDictionary<string, JsonNode?> resolvedParameterValues)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(resolvedEnvironmentVariableValues);
        ArgumentNullException.ThrowIfNull(resolvedParameterValues);

        if (!value.Contains('%', StringComparison.Ordinal))
        {
            return value;
        }

        return PlaceholderPattern.Replace(
            value,
            match =>
            {
                var scope = match.Groups["scope"].Value;
                var key = match.Groups["key"].Value;
                var source = string.Equals(scope, "env", StringComparison.Ordinal)
                    ? resolvedEnvironmentVariableValues
                    : resolvedParameterValues;

                return source.TryGetValue(key, out var resolvedValue)
                    ? resolvedValue?.ToString() ?? string.Empty
                    : match.Value;
            });
    }

    private string? ResolveActionStringProperty(
        string? value,
        string? overrideValue,
        string propertyName,
        string pointer,
        OverlayDiagnostic overlayDiagnostic,
        IDictionary<string, JsonNode?> resolvedEnvironmentVariableValues,
        IDictionary<string, JsonNode?> resolvedParameterValues)
    {
        if (value is null || overrideValue is not null)
        {
            return value;
        }

        var replacedValue = ReplaceValues(value, resolvedEnvironmentVariableValues, resolvedParameterValues);
        var unresolvedPlaceholders = GetUnresolvedPlaceholders(replacedValue);
        if (unresolvedPlaceholders.Count > 0)
        {
            overlayDiagnostic.Warnings.Add(new OpenApiError(
                pointer,
                $"Reusable action reference contains unresolved value placeholders in '{propertyName}': {GetOrderedNames(unresolvedPlaceholders)}."));
        }

        return replacedValue;
    }

    private static HashSet<string> GetUnresolvedPlaceholders(string value)
    {
        var unresolvedPlaceholders = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in PlaceholderPattern.Matches(value))
        {
            unresolvedPlaceholders.Add(match.Value);
        }

        return unresolvedPlaceholders;
    }

    private static string GetOrderedNames(HashSet<string> names)
    {
        var ordered = names.ToArray();
        Array.Sort(ordered, StringComparer.Ordinal);
        return string.Join(", ", ordered);
    }

    [GeneratedRegex("%(?<scope>env|param)\\.(?<key>[A-Za-z][A-Za-z0-9]*)%", RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderRegex();

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