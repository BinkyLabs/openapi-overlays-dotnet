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

        writer.WriteOverlayExtensions(Extensions, OverlaySpecVersion.Overlay1_0);
        writer.WriteEndObject();
    }

    internal bool ApplyToDocument(JsonNode documentJsonNode, OverlayDiagnostic overlayDiagnostic, int index)
    {
        ArgumentNullException.ThrowIfNull(documentJsonNode);
        ArgumentNullException.ThrowIfNull(overlayDiagnostic);
        string GetPointer() => $"$.actions[{index}]";
        if (string.IsNullOrEmpty(Target))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), "Target is required"));
            return false;
        }
        if (Remove is not true && Update is null)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), "Either 'remove' or 'update' must be specified"));
            return false;
        }
        if (Remove is true && Update is not null)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), "'remove' and 'update' cannot be used together"));
            return false;
        }
        if (!JsonPath.TryParse(Target, out var jsonPath))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Invalid JSON Path: '{Target}'"));
            return false;
        }
        if (jsonPath.Evaluate(documentJsonNode) is not { } parseResult)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Target not found: '{Target}'"));
            return false;
        }
        if (Update is not null)
        {
            foreach (var match in parseResult.Matches)
            {
                if (match.Value is null)
                {
                    overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Target '{Target}' does not point to a valid JSON node"));
                    return false;
                }
                MergeJsonNode(match.Value, Update, overlayDiagnostic);
            }
        }
        else if (Remove is true)
        {
            var parentPathString = $"{(jsonPath.Scope is PathScope.Global ? "$" : "@")}{string.Join('.', jsonPath.Segments[..^1].Select(static s => s.ToString()))}";
            if (!JsonPath.TryParse(parentPathString, out var parentPath))
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Invalid parent JSON Path: '{parentPathString}'"));
                return false;
            }
            if (parentPath.Evaluate(documentJsonNode) is not { } parentParseResult)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Parent target not found: '{parentPathString}'"));
                return false;
            }
            if (parentParseResult.Matches.Count < 1)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Parent target '{parentPathString}' must point to at least one JSON node"));
                return false;
            }
            var lastSegment = jsonPath.Segments[^1] ?? throw new InvalidOperationException("Last segment of the JSON Path cannot be null");
            var lastSegmentPath = $"${lastSegment}";
            if (!JsonPath.TryParse(lastSegmentPath, out var lastSegmentPathParsed))
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Invalid last segment JSON Path: '{lastSegmentPath}'"));
                return false;
            }
            foreach (var parentMatch in parentParseResult.Matches)
            {
                if (parentMatch.Value is not JsonNode parentJsonNode)
                {
                    overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Parent target '{parentPathString}' does not point to a valid JSON node"));
                    return false;
                }
                if (lastSegmentPathParsed.Evaluate(parentJsonNode) is not { } lastSegmentParseResult)
                {
                    overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Last segment target not found: '{lastSegmentPath}'"));
                    return false;
                }
                if (lastSegmentParseResult.Matches.Count < 1)
                {
                    overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Last segment target '{lastSegmentPath}' must point to at least one JSON node"));
                    return false;
                }
                if (lastSegmentParseResult.Matches[0].Value is not JsonNode nodeToRemove)
                {
                    overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(), $"Last segment target '{lastSegmentPath}' does not point to a valid JSON node"));
                    return false;
                }
                if (!RemoveJsonNode(parentJsonNode, nodeToRemove, overlayDiagnostic, GetPointer))
                {
                    return false;
                }
            }
        }
        return true;
    }
    private bool RemoveJsonNode(JsonNode parentJsonNode, JsonNode nodeToRemove, OverlayDiagnostic overlayDiagnostic, Func<string> getPointer)
    {
        ArgumentNullException.ThrowIfNull(parentJsonNode);
        ArgumentNullException.ThrowIfNull(nodeToRemove);
        ArgumentNullException.ThrowIfNull(overlayDiagnostic);
        ArgumentNullException.ThrowIfNull(getPointer);
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
        overlayDiagnostic.Errors.Add(new OpenApiError(getPointer(), $"Target '{Target}' does not point to a valid JSON node"));
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