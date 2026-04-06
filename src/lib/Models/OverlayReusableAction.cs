using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a reusable Action Object as defined in the OpenAPI Overlay specification.
/// See: https://spec.openapis.org/overlay/v1.2.0.html#reusable-action-object
/// </summary>
[Experimental("BOO002")]
public class OverlayReusableAction : IOverlayAction
{
    private readonly OverlayCommonAction _common = new();

    /// <inheritdoc/>
    public string? Target
    {
        get => _common.Target;
        set => _common.Target = value;
    }

    /// <inheritdoc/>
    public string? Description
    {
        get => _common.Description;
        set => _common.Description = value;
    }

    /// <inheritdoc/>
    public bool? Remove
    {
        get => _common.Remove;
        set => _common.Remove = value;
    }

    /// <inheritdoc/>
    public JsonNode? Update
    {
        get => _common.Update;
        set => _common.Update = value;
    }

    /// <inheritdoc/>
    public string? Copy
    {
        get => _common.Copy;
        set => _common.Copy = value;
    }

    /// <summary>
    /// The reusable action parameters.
    /// </summary>
    public IList<OverlayReusableActionParameter>? Parameters { get; set; }

    /// <summary>
    /// The reusable action environment variables.
    /// </summary>
    public IList<OverlayReusableActionParameter>? EnvironmentVariables { get; set; }

    /// <summary>
    /// Resolves provided environment variable values against reusable action environment variable definitions.
    /// </summary>
    /// <param name="environmentVariableValues">The provided environment variable values keyed by name.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item>
    /// <description>Resolved environment variable values (provided values plus defaults for missing optional environment variables).</description>
    /// </item>
    /// <item>
    /// <description>A hash set of required environment variable names (no default) that do not have a provided value.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="environmentVariableValues"/> is null.</exception>
    public (Dictionary<string, JsonNode?> ResolvedEnvironmentVariableValues, HashSet<string> MissingRequiredEnvironmentVariableValues) ResolveEnvironmentVariableValues(IDictionary<string, string> environmentVariableValues)
    {
        ArgumentNullException.ThrowIfNull(environmentVariableValues);
        var resolvedEnvironmentVariableValues = new Dictionary<string, JsonNode?>(StringComparer.Ordinal);
        var missingRequiredEnvironmentVariableValues = new HashSet<string>(StringComparer.Ordinal);

        if (EnvironmentVariables is not { Count: > 0 })
        {
            return (resolvedEnvironmentVariableValues, missingRequiredEnvironmentVariableValues);
        }

        var definitionsByName = OverlayReusableActionDefinitionValidator.BuildDefinitionsByName(
            EnvironmentVariables,
            "environment variable");

        if (environmentVariableValues is { Count: > 0 })
        {
            foreach (var environmentVariableValue in environmentVariableValues)
            {
                if (definitionsByName.ContainsKey(environmentVariableValue.Key))
                {
                    resolvedEnvironmentVariableValues[environmentVariableValue.Key] = JsonValue.Create(environmentVariableValue.Value);
                }
            }
        }

        foreach (var definition in definitionsByName)
        {
            if (resolvedEnvironmentVariableValues.ContainsKey(definition.Key))
            {
                continue;
            }

            if (definition.Value.Default is not null)
            {
                resolvedEnvironmentVariableValues[definition.Key] = definition.Value.Default;
                continue;
            }

            missingRequiredEnvironmentVariableValues.Add(definition.Key);
        }

        return (resolvedEnvironmentVariableValues, missingRequiredEnvironmentVariableValues);
    }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions
    {
        get => _common.Extensions;
        set => _common.Extensions = value;
    }

    /// <inheritdoc/>
    public void SerializeAsV1(IOpenApiWriter writer) => _common.SerializeAsV1(writer, SerializeAdditionalPropertiesAsV1);

    /// <inheritdoc/>
    public void SerializeAsV1_1(IOpenApiWriter writer) => _common.SerializeAsV1_1(writer, SerializeAdditionalPropertiesAsV1_1);

    internal OverlayCommonAction CommonAction => _common;

    private void SerializeAdditionalPropertiesAsV1(IOpenApiWriter writer)
    {
        WriteAdditionalProperties(writer, static (w, p) => p.SerializeAsV1(w));
    }

    private void SerializeAdditionalPropertiesAsV1_1(IOpenApiWriter writer)
    {
        WriteAdditionalProperties(writer, static (w, p) => p.SerializeAsV1_1(w));
    }

    private void WriteAdditionalProperties(
        IOpenApiWriter writer,
        Action<IOpenApiWriter, OverlayReusableActionParameter> serializeParameter)
    {
        if (Parameters != null)
        {
            writer.WriteRequiredCollection(
                OverlayConstants.ReusableActionParametersFieldName,
                Parameters,
                serializeParameter);
        }

        if (EnvironmentVariables != null)
        {
            writer.WriteRequiredCollection(
                OverlayConstants.ReusableActionEnvironmentVariablesFieldName,
                EnvironmentVariables,
                serializeParameter);
        }
    }
}