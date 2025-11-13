using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Helper class for processing overlay action parameters.
/// This class is experimental and not part of the OpenAPI Overlay specification v1.0.0.
/// </summary>
[Experimental("BOO002", UrlFormat = "https://github.com/OAI/Overlay-Specification/pull/238")]
internal static partial class ParameterProcessor
{
    // Regex to match ${parameterName} or ${parameterName.key} syntax
    [GeneratedRegex(@"\$\{([a-zA-Z_][a-zA-Z0-9_]*)(?:\.([a-zA-Z_][a-zA-Z0-9_]*))?\}", RegexOptions.Compiled)]
    private static partial Regex ParameterPlaceholderRegex();

    /// <summary>
    /// Expands an action into multiple actions based on parameter combinations.
    /// </summary>
    public static List<OverlayAction> ExpandActionWithParameters(OverlayAction action)
    {
        if (action.Parameters == null || action.Parameters.Count == 0)
        {
            return [action];
        }

        // Resolve parameter values (storing both string representation and objects)
        var resolvedParameters = new Dictionary<string, List<ParameterValue>>();
        foreach (var parameter in action.Parameters)
        {
            if (string.IsNullOrEmpty(parameter.Name))
            {
                continue;
            }

            var values = ResolveParameterValues(parameter);
            if (values != null && values.Count > 0)
            {
                resolvedParameters[parameter.Name] = values;
            }
        }

        // Generate all combinations
        var combinations = GenerateCombinations(resolvedParameters);

        // Create an action for each combination
        var expandedActions = new List<OverlayAction>();
        foreach (var combination in combinations)
        {
            var expandedAction = CloneActionWithInterpolation(action, combination);
            expandedActions.Add(expandedAction);
        }

        return expandedActions;
    }

    /// <summary>
    /// Represents a parameter value that can be either a string or an object.
    /// </summary>
    private class ParameterValue
    {
        public string StringValue { get; set; } = string.Empty;
        public JsonObject? ObjectValue { get; set; }
    }

    /// <summary>
    /// Resolves parameter values from environment variables with fallback to default values.
    /// </summary>
    private static List<ParameterValue>? ResolveParameterValues(OverlayParameter parameter)
    {
        var envVarName = parameter.Name;
        if (string.IsNullOrEmpty(envVarName))
        {
            return null;
        }

        var envValue = Environment.GetEnvironmentVariable(envVarName);
        if (envValue == null)
        {
            // If environment variable is not set, use default values as fallback
            return ExtractParameterValues(parameter.DefaultValues);
        }

        // Try to parse environment variable as JSON
        try
        {
            var parsedEnvValue = JsonNode.Parse(envValue);
            if (parsedEnvValue != null)
            {
                // Validate and extract values from the parsed JSON
                if (!ValidateEnvironmentValue(parsedEnvValue))
                {
                    throw new InvalidOperationException(
                        $"Environment variable '{envVarName}' must be a JSON array of strings or array of objects where each object only contains key/value pairs of strings.");
                }
                return ExtractParameterValues(parsedEnvValue);
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // If it's not valid JSON, treat it as a plain string
            return [new ParameterValue { StringValue = envValue }];
        }

        return [new ParameterValue { StringValue = envValue }];
    }

    /// <summary>
    /// Validates that an environment variable value conforms to the same rules as defaultValues.
    /// </summary>
    private static bool ValidateEnvironmentValue(JsonNode value)
    {
        if (value is not JsonArray array)
        {
            return false;
        }

        if (array.Count == 0)
        {
            return true;
        }

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
    /// Extracts parameter values from the defaultValues JsonNode.
    /// Supports both array of strings and array of objects.
    /// </summary>
    private static List<ParameterValue>? ExtractParameterValues(JsonNode? defaultValues)
    {
        if (defaultValues is not JsonArray array || array.Count == 0)
        {
            return null;
        }

        var result = new List<ParameterValue>();

        foreach (var item in array)
        {
            if (item is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var strValue))
            {
                result.Add(new ParameterValue { StringValue = strValue });
            }
            else if (item is JsonObject jsonObject)
            {
                // For objects, store both the JSON string and the object for dotted access
                result.Add(new ParameterValue
                {
                    StringValue = item.ToJsonString(),
                    ObjectValue = jsonObject
                });
            }
        }

        return result.Count > 0 ? result : null;
    }

    /// <summary>
    /// Generates all combinations of parameter values.
    /// </summary>
    private static List<Dictionary<string, ParameterValue>> GenerateCombinations(Dictionary<string, List<ParameterValue>> parameters)
    {
        if (parameters.Count == 0)
        {
            return [new Dictionary<string, ParameterValue>()];
        }

        var result = new List<Dictionary<string, ParameterValue>>();
        var parameterList = parameters.ToList();

        GenerateCombinationsRecursive(parameterList, 0, new Dictionary<string, ParameterValue>(), result);

        return result;
    }

    /// <summary>
    /// Recursively generates combinations.
    /// </summary>
    private static void GenerateCombinationsRecursive(
        List<KeyValuePair<string, List<ParameterValue>>> parameters,
        int index,
        Dictionary<string, ParameterValue> current,
        List<Dictionary<string, ParameterValue>> result)
    {
        if (index >= parameters.Count)
        {
            result.Add(new Dictionary<string, ParameterValue>(current));
            return;
        }

        var parameter = parameters[index];
        foreach (var value in parameter.Value)
        {
            current[parameter.Key] = value;
            GenerateCombinationsRecursive(parameters, index + 1, current, result);
        }
    }

    /// <summary>
    /// Clones an action and applies string interpolation.
    /// </summary>
    private static OverlayAction CloneActionWithInterpolation(OverlayAction action, Dictionary<string, ParameterValue> parameterValues)
    {
        var clonedAction = new OverlayAction
        {
            Target = InterpolateString(action.Target, parameterValues),
            Description = InterpolateString(action.Description, parameterValues),
            Remove = action.Remove,
            Copy = InterpolateString(action.Copy, parameterValues),
            Update = action.Update != null ? InterpolateJsonNode(action.Update, parameterValues) : null,
            Extensions = action.Extensions
        };

        return clonedAction;
    }

    /// <summary>
    /// Performs string interpolation on a string value.
    /// Supports ${parameterName} and ${parameterName.key} syntax.
    /// </summary>
    private static string? InterpolateString(string? value, Dictionary<string, ParameterValue> parameters)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return ParameterPlaceholderRegex().Replace(value, match =>
        {
            var paramName = match.Groups[1].Value;
            var keyName = match.Groups[2].Success ? match.Groups[2].Value : null;

            if (!parameters.TryGetValue(paramName, out var paramValue))
            {
                return match.Value;
            }

            // If a key is specified and the value is an object, extract the key
            if (keyName != null && paramValue.ObjectValue != null)
            {
                if (paramValue.ObjectValue.TryGetPropertyValue(keyName, out var keyValue) &&
                    keyValue is JsonValue jsonValue &&
                    jsonValue.TryGetValue<string>(out var stringValue))
                {
                    return stringValue;
                }
                return match.Value;
            }

            // Otherwise return the string value
            return paramValue.StringValue;
        });
    }

    /// <summary>
    /// Performs string interpolation on a JSON node.
    /// </summary>
    private static JsonNode InterpolateJsonNode(JsonNode node, Dictionary<string, ParameterValue> parameters)
    {
        if (node is JsonValue jsonValue)
        {
            if (jsonValue.TryGetValue<string>(out var stringValue))
            {
                var interpolated = InterpolateString(stringValue, parameters);
                return JsonValue.Create(interpolated)!;
            }
            return node.DeepClone();
        }
        else if (node is JsonObject jsonObject)
        {
            var newObject = new JsonObject();
            foreach (var kvp in jsonObject)
            {
                newObject[kvp.Key] = kvp.Value != null ? InterpolateJsonNode(kvp.Value, parameters) : null;
            }
            return newObject;
        }
        else if (node is JsonArray jsonArray)
        {
            var newArray = new JsonArray();
            foreach (var item in jsonArray)
            {
                newArray.Add(item != null ? InterpolateJsonNode(item, parameters) : null);
            }
            return newArray;
        }

        return node.DeepClone();
    }
}