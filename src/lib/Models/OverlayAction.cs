using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Writers;

using Json.Path;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;
/// <summary>
/// Represents an Action Object as defined in the OpenAPI Overlay specification v1.0.0.
/// See: https://spec.openapis.org/overlay/v1.0.0.html#action-object
/// </summary>
public class OverlayAction : IOverlaySerializable, IOverlayExtensible
{
    /// <summary>
    /// REQUIRED. The target of the action (JSON Pointer or similar).
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// The description of the action.
    /// </summary>
    public string? Description { get; set; }


    /// <summary>
    /// A boolean value that indicates that the target object or array MUST be removed from the the map or array it is contained in.
    /// The default value is false.
    /// </summary>
    public bool? Remove { get; set; }

    /// <summary>
    /// The update value to be applied to the target.
    /// </summary>
    public JsonNode? Update { get; set; }

    /// <summary>
    /// A string value that indicates that the target object or array MUST be copied to the location indicated by this string, which MUST be a JSON Pointer.
    /// This field is mutually exclusive with the "remove" and "update" fields.
    /// This field is experimental and not part of the OpenAPI Overlay specification v1.0.0.
    /// This field is an implementation of <see href="https://github.com/OAI/Overlay-Specification/pull/150">the copy proposal</see>.
    /// </summary>
    [Experimental("BOO001", UrlFormat = "https://github.com/OAI/Overlay-Specification/pull/150")]
    public string? Copy { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }

    /// <summary>
    /// Serializes the action object as an OpenAPI Overlay v1.0.0 JSON object.
    /// </summary>
    /// <param name="writer">The OpenAPI writer to use for serialization.</param>
    public void SerializeAsV1(IOpenApiWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteRequiredProperty("target", Target);
        writer.WriteProperty("description", Description);
        writer.WriteProperty("remove", Remove, false);

        if (Update != null)
        {
            writer.WriteOptionalObject("update", Update, (w, s) => w.WriteAny(s));
        }
#pragma warning disable BOO001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        if (Copy != null)
        {
            writer.WriteProperty("x-copy", Copy);
        }
#pragma warning restore BOO001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        writer.WriteOverlayExtensions(Extensions, OverlaySpecVersion.Overlay1_0);
        writer.WriteEndObject();
    }

#pragma warning disable BOO001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    internal bool ApplyToDocument(JsonNode documentJsonNode, OverlayDiagnostic overlayDiagnostic, int index)
    {
        ArgumentNullException.ThrowIfNull(documentJsonNode);
        ArgumentNullException.ThrowIfNull(overlayDiagnostic);
        if (string.IsNullOrEmpty(Target))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), "Target is required"));
            return false;
        }
        if (Remove is not true && Update is null && string.IsNullOrEmpty(Copy))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), "At least one of 'remove', 'update' or 'x-copy' must be specified"));
            return false;
        }
        if (Remove is true ^ Update is not null ? !string.IsNullOrEmpty(Copy) : Remove is true)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), "At most one of 'remove', 'update' or 'x-copy' can be specified"));
            return false;
        }
        if (!JsonPath.TryParse(Target, out var jsonPath))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Invalid JSON Path: '{Target}'"));
            return false;
        }
        if (jsonPath.Evaluate(documentJsonNode) is not { } parseResult)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Target not found: '{Target}'"));
            return false;
        }
        if (!string.IsNullOrEmpty(Copy))
        {
            return CopyNodes(parseResult, documentJsonNode, overlayDiagnostic, index);
        }
        else if (Update is not null)
        {
            foreach (var match in parseResult.Matches)
            {
                if (match.Value is null)
                {
                    overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Target '{Target}' does not point to a valid JSON node"));
                    return false;
                }
                MergeJsonNode(match.Value, Update, overlayDiagnostic);
            }
            return true;
        }
        else if (Remove is true)
        {
            return RemoveNodes(documentJsonNode, jsonPath, overlayDiagnostic, index);
        }
        // we should never get here because of the earlier checks
        throw new InvalidOperationException("The action must be either 'remove', 'update' or 'x-copy'");
    }
    private bool CopyNodes(PathResult parseResult, JsonNode documentJsonNode, OverlayDiagnostic overlayDiagnostic, int index)
    {
        if (!JsonPath.TryParse(Copy!, out var copyPath))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Invalid copy JSON Path: '{Copy}'"));
            return false;
        }
        if (copyPath.Evaluate(documentJsonNode) is not { } copyParseResult)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Copy target not found: '{Copy}'"));
            return false;
        }
        if (copyParseResult.Matches.Count < 1)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Copy target '{Copy}' must point to at least one JSON node"));
            return false;
        }

        if (parseResult.Matches.Count != copyParseResult.Matches.Count)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"The number of matches for 'target' ({parseResult.Matches.Count}) and 'x-copy' ({copyParseResult.Matches.Count}) must be the same"));
            return false;
        }
        for (var i = 0; i < copyParseResult.Matches.Count; i++)
        {
            var match = parseResult.Matches[i];
            if (match.Value is null)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Target '{Target}' does not point to a valid JSON node"));
                return false;
            }
            var copyMatch = copyParseResult.Matches[i];
            if (copyMatch.Value is null)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Copy target '{Copy}' does not point to a valid JSON node"));
                return false;
            }
            MergeJsonNode(match.Value, copyMatch.Value, overlayDiagnostic);
        }
        return true;
    }
#pragma warning restore BOO001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    private static string GetPointer(int index) => $"$.actions[{index}]";
    
    private bool RemoveNodes(JsonNode documentJsonNode, JsonPath jsonPath, OverlayDiagnostic overlayDiagnostic, int index)
    {
        var parentPathString = $"{(jsonPath.Scope is PathScope.Global ? "$" : "@")}{string.Concat(jsonPath.Segments[..^1].Select(static s => s.ToString()))}";
        if (!JsonPath.TryParse(parentPathString, out var parentPath))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Invalid parent JSON Path: '{parentPathString}'"));
            return false;
        }
        if (parentPath.Evaluate(documentJsonNode) is not { } parentParseResult)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Parent target not found: '{parentPathString}'"));
            return false;
        }
        if (parentParseResult.Matches.Count < 1)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Parent target '{parentPathString}' must point to at least one JSON node"));
            return false;
        }
        var lastSegment = jsonPath.Segments[^1] ?? throw new InvalidOperationException("Last segment of the JSON Path cannot be null");
        var lastSegmentPath = $"${lastSegment}";
        if (!JsonPath.TryParse(lastSegmentPath, out var lastSegmentPathParsed))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Invalid last segment JSON Path: '{lastSegmentPath}'"));
            return false;
        }
        var parentPathEndsWithWildcard = parentPath.Segments[^1].Selectors.FirstOrDefault() is WildcardSelector;
        var itemRemoved = false;
        foreach (var parentMatch in parentParseResult.Matches)
        {
            if (parentMatch.Value is not JsonNode parentJsonNode)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Parent target '{parentPathString}' does not point to a valid JSON node"));
                return false;
            }
            if (lastSegmentPathParsed.Evaluate(parentJsonNode) is not { } lastSegmentParseResult)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Last segment target not found: '{lastSegmentPath}'"));
                return false;
            }
            if (lastSegmentParseResult.Matches.Count < 1)
            {
                if (parentPathEndsWithWildcard && itemRemoved)
                {
                    // If the parent path ends with a wildcard and we've already removed an item,
                    // it's acceptable for some segments to have no matches.
                    continue;
                }
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Last segment target '{lastSegmentPath}' must point to at least one JSON node"));
                return false;
            }
            if (lastSegmentParseResult.Matches[0].Value is not JsonNode nodeToRemove)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Last segment target '{lastSegmentPath}' does not point to a valid JSON node"));
                return false;
            }
            if (!RemoveJsonNode(parentJsonNode, nodeToRemove, overlayDiagnostic, index))
            {
                return false;
            }
            itemRemoved = true;
        }
        return true;
    }

    private bool RemoveJsonNode(JsonNode parentJsonNode, JsonNode nodeToRemove, OverlayDiagnostic overlayDiagnostic, int index)
    {
        ArgumentNullException.ThrowIfNull(parentJsonNode);
        ArgumentNullException.ThrowIfNull(nodeToRemove);
        ArgumentNullException.ThrowIfNull(overlayDiagnostic);
        if (parentJsonNode is JsonObject currentObject)
        {
            foreach (var kvp in currentObject)
            {
                if (kvp.Value == nodeToRemove)
                {
                    currentObject.Remove(kvp.Key);
                    return true;
                }
            }
        }
        else if (parentJsonNode is JsonArray currentArray)
        {
            for (int i = 0; i < currentArray.Count; i++)
            {
                var currentArrayItem = currentArray[i];
                if (currentArrayItem == nodeToRemove)
                {
                    currentArray.RemoveAt(i);
                    return true;
                }
            }
        }
        overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Target '{Target}' does not point to a valid JSON node"));
        return false;
    }
    private static void MergeJsonNode(JsonNode target, JsonNode update, OverlayDiagnostic overlayDiagnostic)
    {
        if (target is JsonObject targetObject && update is JsonObject updateObject)
        {
            foreach (var kvp in updateObject)
            {
                targetObject[kvp.Key] = kvp.Value?.DeepClone();
            }
        }
        else if (target is JsonArray targetArray && update is JsonArray updateArray)
        {
            targetArray.Clear();
            foreach (var item in updateArray)
            {
                targetArray.Add(item);
            }
        }
        else
        {
            overlayDiagnostic.Errors.Add(new OpenApiError("Update", "Cannot merge non-object or non-array types"));
        }
    }
}