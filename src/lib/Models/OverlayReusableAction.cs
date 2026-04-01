using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a reusable Action Object as defined in the OpenAPI Overlay specification.
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