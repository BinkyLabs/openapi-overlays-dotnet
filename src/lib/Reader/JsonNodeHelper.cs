// Licensed under the MIT license.

using System.Globalization;
using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader;

internal static class JsonNodeHelper
{
    public static JsonObject CheckMapNode(this JsonNode? node, string nodeName, ParsingContext context)
    {
        if (node is not JsonObject jsonObject)
        {
            throw new OverlayReaderException($"{nodeName} must be a map/object", context);
        }

        return jsonObject;
    }

    public static List<T> CreateList<T>(this JsonNode? node, Func<JsonNode, ParsingContext, T> map, ParsingContext context)
    {
        if (node is not JsonArray jsonArray)
        {
            throw new OverlayReaderException($"Expected list while parsing {typeof(T).Name}", context);
        }

        return jsonArray
            .OfType<JsonObject>()
            .Select(n => map(n, context))
            .Where(static i => i != null)
            .ToList();
    }

    public static List<JsonNode> CreateListOfAny(this JsonNode? node, ParsingContext context)
    {
        if (node is not JsonArray jsonArray)
        {
            throw new OverlayReaderException("Cannot create a list from this type of node.", context);
        }

        return jsonArray.OfType<JsonNode>().Select(static n => n.CreateAny()).Where(static i => i != null).ToList();
    }

    public static List<T> CreateSimpleList<T>(this JsonNode? node, Func<JsonNode, T> map, ParsingContext context)
    {
        if (node is not JsonArray jsonArray)
        {
            throw new OverlayReaderException($"Expected list while parsing {typeof(T).Name}", context);
        }

        return jsonArray.OfType<JsonNode>().Select(n =>
        {
            if (n is not JsonValue)
            {
                throw new OverlayReaderException($"Expected a value while parsing at {context.GetLocation()}.");
            }

            return map(n);
        }).ToList();
    }

    public static Dictionary<string, T> CreateMap<T>(this JsonNode? node, Func<JsonNode, ParsingContext, T> map, ParsingContext context)
    {
        if (node is not JsonObject jsonMap)
        {
            throw new OverlayReaderException($"Expected map while parsing {typeof(T).Name}", context);
        }

        var nodes = jsonMap.Select(n =>
        {
            var key = n.Key;
            T value;
            try
            {
                context.StartObject(key);
                value = n.Value is JsonObject jsonObject
                    ? map(jsonObject, context)
                    : default!;
            }
            finally
            {
                context.EndObject();
            }

            return new
            {
                key,
                value
            };
        });

        return nodes.ToDictionary(k => k.key, v => v.value);
    }

    public static Dictionary<string, T> CreateSimpleMap<T>(this JsonNode? node, Func<JsonNode, T> map, ParsingContext context)
    {
        if (node is not JsonObject jsonMap)
        {
            throw new OverlayReaderException($"Expected map while parsing {typeof(T).Name}", context);
        }

        var nodes = jsonMap.Select(n =>
        {
            var key = n.Key;
            try
            {
                context.StartObject(key);
                var jsonNode = n.Value is JsonValue value
                    ? value
                    : throw new OverlayReaderException($"Expected scalar while parsing {typeof(T).Name}", context);

                return (key, value: map(jsonNode));
            }
            finally
            {
                context.EndObject();
            }
        });

        return nodes.ToDictionary(k => k.key, v => v.value);
    }

    public static Dictionary<string, HashSet<T>> CreateArrayMap<T>(this JsonNode? node, Func<JsonNode, T> map, ParsingContext context)
    {
        if (node is not JsonObject jsonMap)
        {
            throw new OverlayReaderException($"Expected map while parsing {typeof(T).Name}", context);
        }

        var nodes = jsonMap.Select(n =>
        {
            var key = n.Key;
            try
            {
                context.StartObject(key);
                var arrayNode = n.Value is JsonArray value
                    ? value
                    : throw new OverlayReaderException($"Expected array while parsing {typeof(T).Name}", context);

                var values = new HashSet<T>(arrayNode.OfType<JsonNode>().Select(map));

                return (key, values);
            }
            finally
            {
                context.EndObject();
            }
        });

        return nodes.ToDictionary(kvp => kvp.key, kvp => kvp.values);
    }

    public static JsonNode CreateAny(this JsonNode? node)
    {
        return node ?? JsonValue.Create((string?)null)!;
    }

    public static string GetRaw(this JsonNode node)
    {
        return node.ToJsonString();
    }

    public static string? GetScalarValue(this JsonNode? node)
    {
        var scalarNode = node is JsonValue value ? value : throw new OpenApiException("Expected scalar value.");

        return Convert.ToString(scalarNode.GetValue<object>(), CultureInfo.InvariantCulture);
    }

    public static T GetScalarValue<T>(this JsonNode? node)
    {
        var scalarNode = node is JsonValue value
            ? value
            : throw new OverlayReaderException("Expected scalar value.");

        return scalarNode.GetValue<T>();
    }

    public static string? GetReferencePointer(this JsonObject jsonObject)
    {
        return jsonObject.TryGetPropertyValue("$ref", out var refNode) ? refNode?.GetScalarValue() : null;
    }

    public static string? GetJsonSchemaIdentifier(this JsonObject jsonObject)
    {
        return jsonObject.TryGetPropertyValue("$id", out var idNode) ? idNode?.GetScalarValue() : null;
    }

    public static string? GetSummaryValue(this JsonObject jsonObject)
    {
        return jsonObject.TryGetPropertyValue("summary", out var summaryNode) ? summaryNode?.GetScalarValue() : null;
    }

    public static string? GetDescriptionValue(this JsonObject jsonObject)
    {
        return jsonObject.TryGetPropertyValue("description", out var descriptionNode) ? descriptionNode?.GetScalarValue() : null;
    }

    public static void ParseMap<T>(
        this JsonObject? jsonObject,
        T domainObject,
        FixedFieldMap<T> fixedFieldMap,
        PatternFieldMap<T> patternFieldMap,
        ParsingContext context)
    {
        if (jsonObject == null)
        {
            return;
        }

        foreach (var propertyNode in jsonObject.Where(static p => p.Value is not null))
        {
            ParseField(propertyNode.Key, propertyNode.Value!, domainObject, fixedFieldMap, patternFieldMap, context);
        }
    }

    private static void ParseField<T>(
        string name,
        JsonNode value,
        T parentInstance,
        FixedFieldMap<T> fixedFields,
        PatternFieldMap<T> patternFields,
        ParsingContext context)
    {
        if (fixedFields.TryGetValue(name, out var fixedFieldMap))
        {
            try
            {
                context.StartObject(name);
                fixedFieldMap(parentInstance, value, context);
            }
            catch (OverlayReaderException ex)
            {
                context.Diagnostic.Errors.Add(new(ex.Pointer, ex.Message));
            }
            catch (OpenApiException ex)
            {
                ex.Pointer = context.GetLocation();
                context.Diagnostic.Errors.Add(new(ex));
            }
            finally
            {
                context.EndObject();
            }
        }
        else
        {
            var map = patternFields.Where(p => p.Key(name)).Select(p => p.Value).FirstOrDefault();
            if (map != null)
            {
                try
                {
                    context.StartObject(name);
                    map(parentInstance, name, value, context);
                }
                catch (OverlayReaderException ex)
                {
                    context.Diagnostic.Errors.Add(new(ex.Pointer, ex.Message));
                }
                catch (OpenApiException ex)
                {
                    ex.Pointer = context.GetLocation();
                    context.Diagnostic.Errors.Add(new(ex));
                }
                finally
                {
                    context.EndObject();
                }
            }
            else if (!"$schema".Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                context.Diagnostic.Errors.Add(new OpenApiError("", $"{name} is not a valid property at {context.GetLocation()}"));
            }
        }
    }
}