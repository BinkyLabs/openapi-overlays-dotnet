// Licensed under the MIT license.

using System.Text.Json.Nodes;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays.Reader;

/// <summary>
/// Wrapper class around JsonDocument to isolate semantic parsing from details of Json DOM.
/// </summary>
internal class RootNode : ParseNode
{
    private readonly JsonNode _jsonNode;

    public RootNode(
        ParsingContext context,
        JsonNode jsonNode) : base(context, jsonNode)
    {
        _jsonNode = jsonNode;
    }

    /// <summary>
    /// Finds a node in the JSON document by reference pointer.
    /// </summary>
    /// <param name="referencePointer">The JSON pointer to search for.</param>
    /// <returns>The found <see cref="ParseNode"/> or null.</returns>
    public ParseNode? Find(JsonPointer referencePointer)
    {
        if (referencePointer.Find(_jsonNode) is not JsonNode jsonNode)
        {
            return null;
        }

        return Create(Context, jsonNode);
    }

    /// <summary>
    /// Gets the root map node.
    /// </summary>
    /// <returns>The <see cref="MapNode"/> representing the root object.</returns>
    public MapNode GetMap()
    {
        return new MapNode(Context, _jsonNode);
    }
}