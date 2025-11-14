using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
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
    private record ParameterValue
    {
        public string StringValue { get; init; } = string.Empty;
        public JsonObject? ObjectValue { get; init; }
        public static ParameterValue FromJsonNode(JsonNode node)
        {
            return node switch
            {
                JsonValue jsonValue when jsonValue.TryGetValue<string>(out var str) && !string.IsNullOrEmpty(str) => new ParameterValue { StringValue = str },
                JsonObject jsonObject => new ParameterValue { ObjectValue = jsonObject },
                _ => throw new InvalidOperationException("Invalid parameter value type."),
            };
        }
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
        var jsonArray = string.IsNullOrEmpty(envValue) switch
        {
            false when TryParseJsonValue(envValue, out var parsedEnvValues) => parsedEnvValues,
            false => new JsonArray(envValue),
            true when parameter.DefaultValues is JsonArray defaultValues => defaultValues,
            _ => null,
        };
        return jsonArray == null ? null : jsonArray.OfType<JsonNode>().Select(ParameterValue.FromJsonNode).ToList();
    }

    private static bool TryParseJsonValue(string value, [NotNullWhen(true)] out JsonArray? parameterValues)
    {
        try
        {
            if (JsonNode.Parse(value) is JsonArray parsedValue)
            {
                if (!OverlayParameter.AreDefaultValuesValid(parsedValue))
                    throw new InvalidOperationException("Invalid parameter values format.");
                parameterValues = parsedValue;
                return true;
            }
        }
        catch (JsonException)
        {
            // TODO log parsing error, we need a logging infrastructure though
        }
        parameterValues = null;
        return false;
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
            var paramKey = match.Groups.Count > 2 && match.Groups[2].Success ? match.Groups[2].Value : null;
            return (string.IsNullOrEmpty(paramName), string.IsNullOrEmpty(paramKey), parameters.TryGetValue(paramName, out var value)) switch
            {
                (false, true, true) when !string.IsNullOrEmpty(value?.StringValue) => value.StringValue,
                (false, false, true) when value is { ObjectValue: JsonObject objectValue } &&
                                        objectValue.TryGetPropertyValue(paramKey!, out var compositeValue) &&
                                        compositeValue is JsonValue jsonValue &&
                                        jsonValue.TryGetValue<string>(out var compositeStringValue) => compositeStringValue,
                (_, _, _) => match.Value,
            };
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