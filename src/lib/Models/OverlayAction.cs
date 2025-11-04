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
    /// This field is mutually exclusive with the <see cref="Remove"/> and <see cref="Update"/> fields.
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

    private (bool, JsonPath?, PathResult?) ValidateBeforeApplying(JsonNode documentJsonNode, OverlayDiagnostic overlayDiagnostic, int index)
    {
        if (string.IsNullOrEmpty(Target))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), "Target is required"));
            return (false, null, null);
        }
        if (Remove is not true && Update is null && string.IsNullOrEmpty(Copy))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), "At least one of 'remove', 'update' or 'x-copy' must be specified"));
            return (false, null, null);
        }
        if (Remove is true ^ Update is not null ? !string.IsNullOrEmpty(Copy) : Remove is true)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), "At most one of 'remove', 'update' or 'x-copy' can be specified"));
            return (false, null, null);
        }
        if (!JsonPath.TryParse(Target, out var jsonPath))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Invalid JSON Path: '{Target}'"));
            return (false, null, null);
        }
        if (jsonPath.Evaluate(documentJsonNode) is not { } parseResult)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Target not found: '{Target}'"));
            return (false, null, null);
        }
        return (true, jsonPath, parseResult);
    }

    internal bool ApplyToDocument(JsonNode documentJsonNode, OverlayDiagnostic overlayDiagnostic, int index)
    {
        ArgumentNullException.ThrowIfNull(documentJsonNode);
        ArgumentNullException.ThrowIfNull(overlayDiagnostic);

        var (isValid, jsonPath, parseResult) = ValidateBeforeApplying(documentJsonNode, overlayDiagnostic, index);
        if (!isValid || parseResult is null || jsonPath is null)
        {
            return false;
        }

        try
        {
            if (!string.IsNullOrEmpty(Copy))
            {
                return CopyNodes(parseResult, documentJsonNode, overlayDiagnostic, index);
            }
            else if (Update is not null)
            {
                return UpdateNodes(parseResult, overlayDiagnostic, index);
            }
            else if (Remove is true)
            {
                return RemoveNodes(parseResult, documentJsonNode, jsonPath, overlayDiagnostic, index);
            }
            // we should never get here because of the earlier checks
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Error occurred when updating target '{Target}': The action must be either 'remove', 'update' or 'x-copy'"));
            return false;
        }
        catch (Exception ex)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Error occurred when updating target '{Target}': {ex.Message}"));
            return false;
        }
    }
    private bool UpdateNodes(PathResult parseResult, OverlayDiagnostic overlayDiagnostic, int index)
    {
        // Matches are evaluated by the enumerator on demand, so we need to force evaluation here to avoid stack overflows in some contexts
        var matchValues = parseResult.Matches.Select(static m => m.Value).ToArray();
        if (matchValues.Any(static m => m is null))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Target '{Target}' does not point to a valid JSON node"));
            return false;
        }
        foreach (var match in matchValues)
        {
            MergeJsonNode(match!, Update!, overlayDiagnostic);
        }
        return true;
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
        if (copyParseResult.Matches.Count != 1)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Copy JSON Path '{Copy}' must match exactly one result, but matched {copyParseResult.Matches.Count}"));
            return false;
        }

        var matchValues = parseResult.Matches.Select(static m => m.Value).ToArray();
        if (matchValues.Any(static m => m is null))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Target '{Target}' does not point to a valid JSON node"));
            return false;
        }
        if (copyParseResult.Matches[0].Value is not { } copyMatch)
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Copy target '{Copy}' does not point to a valid JSON node"));
            return false;
        }
        // Copy the same source to all targets
        foreach (var match in matchValues)
        {
            MergeJsonNode(match!, copyMatch, overlayDiagnostic);
        }
        return true;
    }
#pragma warning restore BOO001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    private static string GetPointer(int index) => $"$.actions[{index}]";

    private bool RemoveNodes(PathResult parseResult, JsonNode documentJsonNode, JsonPath jsonPath, OverlayDiagnostic overlayDiagnostic, int index)
    {
        var lastSegment = jsonPath.Segments[^1] ?? throw new InvalidOperationException("Last segment of the JSON Path cannot be null");
        var lastSegmentPath = $"${lastSegment}";
        if (!JsonPath.TryParse(lastSegmentPath, out var lastSegmentPathParsed))
        {
            overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Invalid last segment JSON Path: '{lastSegmentPath}'"));
            return false;
        }
        // Matches are evaluated by the enumerator on demand, so we need to force evaluation here to avoid stack overflows in some contexts
        var matches = parseResult.Matches.ToArray();
        foreach (var match in matches)
        {
            if (match.Location is null || match.Value is null)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Target '{Target}' does not point to a valid JSON node"));
                return false;
            }
            if (!Json.Pointer.JsonPointer.TryParse(match.Location.AsJsonPointer(), out var currentJsonPointer))
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Target '{Target}' does not point to a valid JSON node"));
                return false;
            }
            if (!currentJsonPointer.GetAncestor(1).TryEvaluate(documentJsonNode, out var parentJsonNode) || parentJsonNode is null)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Could not find the parent node for '{Target}'"));
                return false;
            }
            if (lastSegmentPathParsed.Evaluate(parentJsonNode) is not { } lastSegmentParseResult)
            {
                overlayDiagnostic.Errors.Add(new OpenApiError(GetPointer(index), $"Last segment target not found: '{lastSegmentPath}'"));
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
                if (kvp.Value is null)
                {
                    targetObject[kvp.Key] = null;
                    continue;
                }
                var objectTargetValue = targetObject[kvp.Key];
                if (objectTargetValue is null)
				{
                    targetObject[kvp.Key] = kvp.Value.DeepClone();
                    continue;
				}
                MergeJsonNode(objectTargetValue, kvp.Value, overlayDiagnostic);
            }
        }
        else if (target is JsonArray targetArray && update is JsonArray updateArray)
        {
            foreach (var item in updateArray)
            {
                targetArray.Add(item?.DeepClone());
            }
        }
        else if (target is JsonValue && update is JsonValue)
        {
            ReplaceValueInParent(target, update);
        }
        else
        {
            overlayDiagnostic.Errors.Add(new OpenApiError("Update", "Cannot merge incompatible types"));
        }
    }

    private static void ReplaceValueInParent(JsonNode target, JsonNode update)
    {
        var parent = target.Parent;
        if (parent is JsonObject parentObject)
        {
            // Find the property name for this target
            foreach (var kvp in parentObject)
            {
                if (kvp.Value == target)
                {
                    parentObject[kvp.Key] = update.DeepClone();
                    return;
                }
            }
        }
        else if (parent is JsonArray parentArray)
        {
            // Find the index for this target
            for (int i = 0; i < parentArray.Count; i++)
            {
                if (parentArray[i] == target)
                {
                    parentArray[i] = update.DeepClone();
                    return;
                }
            }
        }
    }
}