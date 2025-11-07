using System.Diagnostics.CodeAnalysis;

using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a parameter for an overlay action.
/// This class is experimental and not part of the OpenAPI Overlay specification v1.0.0.
/// This class is an implementation of <see href="https://github.com/OAI/Overlay-Specification/pull/238">the action parameters proposal</see>.
/// </summary>
[Experimental("BOO002", UrlFormat = "https://github.com/OAI/Overlay-Specification/pull/238")]
public class OverlayParameter : IOverlaySerializable
{
    /// <summary>
    /// REQUIRED. The name of the parameter.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The source of the parameter value. Defaults to Inline.
    /// </summary>
    public ParameterValueSource Source { get; set; } = ParameterValueSource.Inline;

    /// <summary>
    /// The values for the parameter. For Inline source, this is required.
    /// For Environment source, this is optional and overrides the environment variable value.
    /// </summary>
    public List<string>? Values { get; set; }

    /// <summary>
    /// The separator to use when splitting environment variable values into multiple values.
    /// Only applies when Source is Environment.
    /// </summary>
    public string? Separator { get; set; }

    /// <summary>
    /// Serializes the parameter object as an OpenAPI Overlay v1.0.0 JSON object.
    /// </summary>
    /// <param name="writer">The OpenAPI writer to use for serialization.</param>
    public void SerializeAsV1(IOpenApiWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteRequiredProperty("name", Name);
        writer.WriteProperty("source", Source.ToString().ToLowerInvariant());

        if (Values != null && Values.Count > 0)
        {
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var value in Values)
            {
                writer.WriteValue(value);
            }
            writer.WriteEndArray();
        }

        writer.WriteProperty("separator", Separator);
        writer.WriteEndObject();
    }
}