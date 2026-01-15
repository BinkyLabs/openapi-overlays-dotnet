using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

#pragma warning disable BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

internal static partial class OverlayV1Deserializer
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JsonNode operations are required for dynamic parameter values")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JsonNode operations are required for dynamic parameter values")]
    private static JsonNode? ParseDefaultValues(ParseNode node)
    {
        if (node is ListNode listNode)
        {
            var jsonArray = new JsonArray();
            foreach (var item in listNode)
            {
                if (item is ValueNode valueNode)
                {
                    var scalarValue = valueNode.GetScalarValue();
                    var jsonValue = JsonNode.Parse($"\"{scalarValue}\"");
                    if (jsonValue != null)
                    {
                        jsonArray.Add(jsonValue);
                    }
                }
                else if (item is MapNode mapNode)
                {
                    var jsonObject = new JsonObject();
                    foreach (var propertyNode in mapNode)
                    {
                        var propValue = propertyNode.Value.GetScalarValue();
                        if (propValue != null)
                        {
                            var jsonValue = JsonNode.Parse($"\"{propValue}\"");
                            if (jsonValue != null)
                            {
                                jsonObject[propertyNode.Name] = jsonValue;
                            }
                        }
                    }
                    jsonArray.Add(jsonObject);
                }
            }

            // Validate the array
            if (!ValidateDefaultValues(jsonArray))
            {
                throw new OverlayReaderException(
                    "DefaultValues must be an array of strings or an array of objects where each object only contains key/value pairs of strings.");
            }

            return jsonArray;
        }

        return null;
    }

    private static bool ValidateDefaultValues(JsonArray array)
    {
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

    public static readonly FixedFieldMap<OverlayParameter> ParameterFixedFields = new()
    {
        { "name", (o, v) => o.Name = v.GetScalarValue() },
        { "defaultValues", (o, v) => o.DefaultValues = ParseDefaultValues(v) },
    };

    public static readonly PatternFieldMap<OverlayParameter> ParameterPatternFields = new()
    {
        // Parameters don't have extension fields
    };

    public static OverlayParameter LoadParameter(ParseNode node)
    {
        var mapNode = node.CheckMapNode("Parameter");
        var parameter = new OverlayParameter();
        ParseMap(mapNode, parameter, ParameterFixedFields, ParameterPatternFields);

        return parameter;
    }
}

#pragma warning restore BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.