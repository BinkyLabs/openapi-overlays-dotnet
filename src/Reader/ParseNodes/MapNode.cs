﻿
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader
{
    /// <summary>
    /// Abstraction of a Map to isolate semantic parsing from details of JSON DOM
    /// </summary>
    internal class MapNode : ParseNode, IEnumerable<PropertyNode>
    {
        private readonly JsonObject _node;
        private readonly List<PropertyNode> _nodes;

        public MapNode(ParsingContext context, JsonNode node) : base(
            context, node)
        {
            if (node is not JsonObject mapNode)
            {
                throw new OverlayReaderException("Expected map.", Context);
            }

            _node = mapNode;
            _nodes = _node.Where(p => p.Value is not null).OfType<KeyValuePair<string, JsonNode>>().Select(p => new PropertyNode(Context, p.Key, p.Value)).ToList();
        }

        public PropertyNode? this[string key]
        {
            get
            {
                if (_node.TryGetPropertyValue(key, out var node) && node is not null)
                {
                    return new(Context, key, node);
                }

                return null;
            }
        }

        public override Dictionary<string, T> CreateMap<T>(Func<MapNode, T> map)
        {
            var jsonMap = _node ?? throw new OverlayReaderException($"Expected map while parsing {typeof(T).Name}", Context);
            var nodes = jsonMap.Select(
                n =>
                {

                    var key = n.Key;
                    T value;
                    try
                    {
                        Context.StartObject(key);
                        value = n.Value is JsonObject jsonObject
                          ? map(new MapNode(Context, jsonObject))
                          : default!;
                    }
                    finally
                    {
                        Context.EndObject();
                    }
                    return new
                    {
                        key,
                        value
                    };
                });

            return nodes.ToDictionary(k => k.key, v => v.value);
        }

        public override Dictionary<string, T> CreateSimpleMap<T>(Func<ValueNode, T> map)
        {
            var jsonMap = _node ?? throw new OverlayReaderException($"Expected map while parsing {typeof(T).Name}", Context);
            var nodes = jsonMap.Select(
                n =>
                {
                    var key = n.Key;
                    try
                    {
                        Context.StartObject(key);
                        JsonValue valueNode = n.Value is JsonValue value ? value
                        : throw new OverlayReaderException($"Expected scalar while parsing {typeof(T).Name}", Context);

                        return (key, value: map(new ValueNode(Context, valueNode)));
                    }
                    finally
                    {
                        Context.EndObject();
                    }
                });

            return nodes.ToDictionary(k => k.key, v => v.value);
        }

        public override Dictionary<string, HashSet<T>> CreateArrayMap<T>(Func<ValueNode, T> map)
        {
            var jsonMap = _node ?? throw new OverlayReaderException($"Expected map while parsing {typeof(T).Name}", Context);

            var nodes = jsonMap.Select(n =>
            {
                var key = n.Key;
                try
                {
                    Context.StartObject(key);
                    JsonArray arrayNode = n.Value is JsonArray value
                        ? value
                        : throw new OverlayReaderException($"Expected array while parsing {typeof(T).Name}", Context);

                    HashSet<T> values = new HashSet<T>(arrayNode.OfType<JsonNode>().Select(item => map(new ValueNode(Context, item))));

                    return (key, values);

                }
                finally
                {
                    Context.EndObject();
                }
            });

            return nodes.ToDictionary(kvp => kvp.key, kvp => kvp.values);
        }

        public IEnumerator<PropertyNode> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        public override string GetRaw()
        {
            var x = JsonSerializer.Serialize(_node, SourceGenerationContext.Default.JsonObject);
            return x;
        }

        public string? GetReferencePointer()
        {
            if (!_node.TryGetPropertyValue("$ref", out JsonNode? refNode))
            {
                return null;
            }

            return refNode?.GetScalarValue();
        }

        public string? GetJsonSchemaIdentifier()
        {
            if (!_node.TryGetPropertyValue("$id", out JsonNode? idNode))
            {
                return null;
            }

            return idNode?.GetScalarValue();
        }

        public string? GetSummaryValue()
        {
            if (!_node.TryGetPropertyValue("summary", out JsonNode? summaryNode))
            {
                return null;
            }

            return summaryNode?.GetScalarValue();
        }

        public string? GetDescriptionValue()
        {
            if (!_node.TryGetPropertyValue("description", out JsonNode? descriptionNode))
            {
                return null;
            }

            return descriptionNode?.GetScalarValue();
        }

        public string? GetScalarValue(ValueNode key)
        {
            var keyValue = key.GetScalarValue();
            if (keyValue is not null)
            {
                var scalarNode = _node[keyValue] is JsonValue jsonValue
                        ? jsonValue
                        : throw new OverlayReaderException($"Expected scalar while parsing {key.GetScalarValue()}", Context);

                return Convert.ToString(scalarNode?.GetValue<object>(), CultureInfo.InvariantCulture);
            }
            return null;
        }

        /// <summary>
        /// Create an <see cref="JsonNodeExtension"/>
        /// </summary>
        /// <returns>The created Json object.</returns>
        public override JsonNode CreateAny()
        {
            return _node;
        }
    }

    [JsonSerializable(typeof(JsonObject))]
    internal partial class SourceGenerationContext : JsonSerializerContext { }
}