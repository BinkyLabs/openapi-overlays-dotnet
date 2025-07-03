// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays;
using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// A wrapper class for JsonNode
/// </summary>
public class JsonNodeExtension : IOpenApiElement, IOverlayExtension
{
    private readonly JsonNode jsonNode;

    /// <summary>
    /// Initializes the <see cref="JsonNodeExtension"/> class.
    /// </summary>
    /// <param name="jsonNode"></param>
    public JsonNodeExtension(JsonNode jsonNode)
    {
        this.jsonNode = jsonNode;
    }

    /// <summary>
    /// Gets the underlying JsonNode.
    /// </summary>
    public JsonNode Node { get { return jsonNode; } }

    /// <inheritdoc/>
    public void Write(IOpenApiWriter writer, OverlaySpecVersion specVersion)
    {
        writer.WriteAny(Node);
    }
}
