﻿
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader
{
    internal class ListNode : ParseNode, IEnumerable<ParseNode>
    {
        private readonly JsonArray _nodeList;

        public ListNode(ParsingContext context, JsonArray jsonArray) : base(
            context, jsonArray)
        {
            _nodeList = jsonArray;
        }

        public override List<T> CreateList<T>(Func<MapNode, T> map)
        {
            if (_nodeList == null)
            {
                throw new OverlayReaderException($"Expected list while parsing {typeof(T).Name}");
            }

            var list = _nodeList
                .OfType<JsonObject>()
                .Select(n => map(new MapNode(Context, n)))
                .Where(i => i != null)
                .ToList();
            return list;
        }

        public override List<JsonNode> CreateListOfAny()
        {

            var list = _nodeList.OfType<JsonNode>().Select(n => Create(Context, n).CreateAny())
                .Where(i => i != null)
                .ToList();

            return list;
        }

        public override List<T> CreateSimpleList<T>(Func<ValueNode, T> map)
        {
            if (_nodeList == null)
            {
                throw new OverlayReaderException($"Expected list while parsing {typeof(T).Name}");
            }

            return _nodeList.OfType<JsonNode>().Select(n => map(new(Context, n))).ToList();
        }

        public IEnumerator<ParseNode> GetEnumerator()
        {
            return _nodeList.OfType<JsonNode>().Select(n => Create(Context, n)).ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Create a <see cref="JsonArray"/>
        /// </summary>
        /// <returns>The created Any object.</returns>
        public override JsonNode CreateAny()
        {
            return _nodeList;
        }
    }
}