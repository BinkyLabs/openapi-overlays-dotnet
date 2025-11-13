using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

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
    /// Must be an array of strings or an array of objects where each object only contains key/value pairs of strings.
    /// </summary>
    public JsonNode? DefaultValues { get; set; }

    /// <summary>
    /// Validates that defaultValues is either an array of strings or an array of objects with string key/value pairs.
    /// </summary>
    private static bool ValidateDefaultValues(JsonNode? defaultValues)
    {
        if (defaultValues == null)
        {
            return true;
        }

        if (defaultValues is not JsonArray array)
        {
            return false;
        }

        if (array.Count == 0)
        {
            return true;
        }

        // Check if all elements are strings
        var allStrings = true;
        var allObjects = true;

        foreach (var item in array)
        {
            if (item == null)
            {
                return false;
            }

            if (item is JsonValue jsonValue)
            {
                allObjects = false;
                if (!jsonValue.TryGetValue<string>(out _))
                {
                    allStrings = false;
                }
            }
            else if (item is JsonObject jsonObject)
            {
                allStrings = false;
                // Validate that all properties have string values
                foreach (var prop in jsonObject)
                {
                    if (prop.Value == null || prop.Value is not JsonValue propValue || !propValue.TryGetValue<string>(out _))
                    {
                        allObjects = false;
                        break;
                    }
                }
            }
            else
            {
                return false;
            }

            if (!allStrings && !allObjects)
            {
                return false;
            }
        }

        return allStrings || allObjects;
    }

    /// <summary>
    /// Serializes the parameter object as an OpenAPI Overlay v1.0.0 JSON object.
    /// </summary>
    /// <param name="writer">The OpenAPI writer to use for serialization.</param>
    public void SerializeAsV1(IOpenApiWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteRequiredProperty("name", Name);

        if (DefaultValues != null)
        {
            if (!ValidateDefaultValues(DefaultValues))
            {
                throw new InvalidOperationException(
                    "DefaultValues must be an array of strings or an array of objects where each object only contains key/value pairs of strings.");
            }

            writer.WritePropertyName("defaultValues");
            writer.WriteAny(DefaultValues);
        }

        writer.WriteEndObject();
    }
}