using System.Diagnostics.CodeAnalysis;

using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a reusable Action Object as defined in the OpenAPI Overlay specification.
/// See: https://spec.openapis.org/overlay/v1.2.0.html#reusable-action-object
/// </summary>
[Experimental("BOO002")]
public class OverlayReusableAction : IOverlaySerializable, IOverlayExtensible
{
    /// <summary>
    /// The action fields for this reusable action.
    /// </summary>
    public OverlayAction? Fields { get; set; }

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
    public (Dictionary<string, string> ResolvedEnvironmentVariableValues, HashSet<string> MissingRequiredEnvironmentVariableValues) ResolveEnvironmentVariableValues(IDictionary<string, string> environmentVariableValues)
    {
        ArgumentNullException.ThrowIfNull(environmentVariableValues);
        var missingRequiredEnvironmentVariableValues = new HashSet<string>(StringComparer.Ordinal);

        if (EnvironmentVariables is not { Count: > 0 })
        {
            return ([], missingRequiredEnvironmentVariableValues);
        }

        var definitionsByName = OverlayReusableActionDefinitionValidator.BuildDefinitionsByName(
            EnvironmentVariables,
            "environment variable");

        var resolvedEnvironmentVariableValues = new Dictionary<string, string>(
            environmentVariableValues is { Count: > 0 } ? environmentVariableValues.Where(x => definitionsByName.ContainsKey(x.Key)) : [],
            StringComparer.Ordinal);

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
    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }

    /// <inheritdoc/>
    public void SerializeAsV1(IOpenApiWriter writer) =>
        SerializeInternal(writer, OverlaySpecVersion.Overlay1_0, static (w, p) => p.SerializeAsV1(w), static (w, f) => f.SerializeFieldsAsV1(w));

    /// <inheritdoc/>
    public void SerializeAsV1_1(IOpenApiWriter writer) =>
        SerializeInternal(writer, OverlaySpecVersion.Overlay1_1, static (w, p) => p.SerializeAsV1_1(w), static (w, f) => f.SerializeFieldsAsV1_1(w));

    private void SerializeInternal(
        IOpenApiWriter writer,
        OverlaySpecVersion version,
        Action<IOpenApiWriter, OverlayReusableActionParameter> serializeParameter,
        Action<IOpenApiWriter, OverlayAction> serializeFields)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartObject();

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

        writer.WriteRequiredObject(
            OverlayConstants.ReusableActionFieldsFieldName,
            Fields ?? new OverlayAction(),
            serializeFields);

        writer.WriteOverlayExtensions(Extensions, version);
        writer.WriteEndObject();
    }
}