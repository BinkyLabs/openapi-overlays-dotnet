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
    /// Default values for the parameter when the environment variable is not set.
    /// </summary>
    public List<string>? DefaultValues { get; set; }

    /// <summary>
    /// The separator to use when splitting environment variable values into multiple values.
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

        if (DefaultValues != null && DefaultValues.Count > 0)
        {
            writer.WritePropertyName("defaultValues");
            writer.WriteStartArray();
            foreach (var value in DefaultValues)
            {
                writer.WriteValue(value);
            }
            writer.WriteEndArray();
        }

        // Only write separator if it's not null or empty
        if (!string.IsNullOrEmpty(Separator))
        {
            writer.WriteProperty("separator", Separator);
        }

        writer.WriteEndObject();
    }
}