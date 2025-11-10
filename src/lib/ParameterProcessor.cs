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
    // Regex to match ${parameterName} syntax
    [GeneratedRegex(@"\$\{([a-zA-Z_][a-zA-Z0-9_]*)\}", RegexOptions.Compiled)]
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

        // Resolve parameter values
        var resolvedParameters = new Dictionary<string, List<string>>();
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
    /// Resolves parameter values from environment variables with fallback to default values.
    /// </summary>
    private static List<string>? ResolveParameterValues(OverlayParameter parameter)
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
            return parameter.DefaultValues;
        }

        // Split by separator if provided
        if (!string.IsNullOrEmpty(parameter.Separator))
        {
            return [.. envValue.Split(parameter.Separator, StringSplitOptions.RemoveEmptyEntries)];
        }

        return [envValue];
    }

    /// <summary>
    /// Generates all combinations of parameter values.
    /// </summary>
    private static List<Dictionary<string, string>> GenerateCombinations(Dictionary<string, List<string>> parameters)
    {
        if (parameters.Count == 0)
        {
            return [new Dictionary<string, string>()];
        }

        var result = new List<Dictionary<string, string>>();
        var parameterList = parameters.ToList();

        GenerateCombinationsRecursive(parameterList, 0, new Dictionary<string, string>(), result);

        return result;
    }

    /// <summary>
    /// Recursively generates combinations.
    /// </summary>
    private static void GenerateCombinationsRecursive(
        List<KeyValuePair<string, List<string>>> parameters,
        int index,
        Dictionary<string, string> current,
        List<Dictionary<string, string>> result)
    {
        if (index >= parameters.Count)
        {
            result.Add(new Dictionary<string, string>(current));
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
    private static OverlayAction CloneActionWithInterpolation(OverlayAction action, Dictionary<string, string> parameterValues)
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
    /// </summary>
    private static string? InterpolateString(string? value, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return ParameterPlaceholderRegex().Replace(value, match =>
        {
            var paramName = match.Groups[1].Value;
            return parameters.TryGetValue(paramName, out var paramValue) ? paramValue : match.Value;
        });
    }

    /// <summary>
    /// Performs string interpolation on a JSON node.
    /// </summary>
    private static JsonNode InterpolateJsonNode(JsonNode node, Dictionary<string, string> parameters)
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