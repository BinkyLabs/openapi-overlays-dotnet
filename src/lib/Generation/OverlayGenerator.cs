using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Generation;

/// <summary>
/// Generates overlay documents by comparing two OpenAPI documents.
/// </summary>
public static class OverlayGenerator
{
    /// <summary>
    /// Generates an overlay document that represents the differences between two OpenAPI documents.
    /// </summary>
    /// <param name="sourceDocument">The source (original) document as JsonNode.</param>
    /// <param name="targetDocument">The target (modified) document as JsonNode.</param>
    /// <returns>A ReadResult containing the overlay document and any diagnostics.</returns>
    public static ReadResult Generate(JsonNode sourceDocument, JsonNode targetDocument)
    {
        return Generate(sourceDocument, targetDocument, null);
    }

    /// <summary>
    /// Generates an overlay document that represents the differences between two OpenAPI documents.
    /// </summary>
    /// <param name="sourceDocument">The source (original) document as JsonNode.</param>
    /// <param name="targetDocument">The target (modified) document as JsonNode.</param>
    /// <param name="info">Overlay info metadata.</param>
    /// <returns>A ReadResult containing the overlay document and any diagnostics.</returns>
    public static ReadResult Generate(JsonNode sourceDocument, JsonNode targetDocument, OverlayInfo? info)
    {
        ArgumentNullException.ThrowIfNull(sourceDocument);
        ArgumentNullException.ThrowIfNull(targetDocument);

        var diagnostic = new OverlayDiagnostic();

        if (!ValidateOpenApiVersions(sourceDocument, targetDocument, diagnostic))
        {
            return new ReadResult
            {
                Document = null,
                Diagnostic = diagnostic
            };
        }

        var actions = new List<OverlayAction>();
        GenerateDiff(sourceDocument, targetDocument, "$", actions);

        var document = new OverlayDocument
        {
            Info = info ?? new OverlayInfo
            {
                Title = "Generated Overlay",
                Version = "1.0.0"
            },
            Actions = actions
        };

        return new ReadResult
        {
            Document = document,
            Diagnostic = diagnostic
        };
    }

    /// <summary>
    /// Generates an overlay document by loading two OpenAPI documents from streams.
    /// </summary>
    /// <param name="sourceStream">Stream containing the source document.</param>
    /// <param name="targetStream">Stream containing the target document.</param>
    /// <returns>A ReadResult containing the overlay document and any diagnostics.</returns>
    public static Task<ReadResult> GenerateFromStreamsAsync(
        Stream sourceStream,
        Stream targetStream)
    {
        return GenerateFromStreamsAsync(sourceStream, targetStream, null, null, null, CancellationToken.None);
    }

    /// <summary>
    /// Generates an overlay document by loading two OpenAPI documents from streams.
    /// </summary>
    /// <param name="sourceStream">Stream containing the source document.</param>
    /// <param name="targetStream">Stream containing the target document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A ReadResult containing the overlay document and any diagnostics.</returns>
    public static Task<ReadResult> GenerateFromStreamsAsync(
        Stream sourceStream,
        Stream targetStream,
        CancellationToken cancellationToken)
    {
        return GenerateFromStreamsAsync(sourceStream, targetStream, null, null, null, cancellationToken);
    }

    /// <summary>
    /// Generates an overlay document by loading two OpenAPI documents from streams.
    /// </summary>
    /// <param name="sourceStream">Stream containing the source document.</param>
    /// <param name="targetStream">Stream containing the target document.</param>
    /// <param name="format">The format of the documents (json or yaml).</param>
    /// <param name="info">Overlay info metadata.</param>
    /// <param name="readerSettings">Reader settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A ReadResult containing the overlay document and any diagnostics.</returns>
    public static async Task<ReadResult> GenerateFromStreamsAsync(
        Stream sourceStream,
        Stream targetStream,
        string? format,
        OverlayInfo? info,
        OverlayReaderSettings? readerSettings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);
        ArgumentNullException.ThrowIfNull(targetStream);

        readerSettings ??= new OverlayReaderSettings();

        if (string.IsNullOrEmpty(format))
        {
            format = "json";
        }

        var reader = readerSettings.GetReader(format) ?? throw new NotSupportedException($"No reader found for format '{format}'.");

        var sourceNode = await reader.GetJsonNodeFromStreamAsync(sourceStream, cancellationToken).ConfigureAwait(false);
        var targetNode = await reader.GetJsonNodeFromStreamAsync(targetStream, cancellationToken).ConfigureAwait(false);

        if (sourceNode is null)
        {
            throw new InvalidOperationException("Failed to parse source document.");
        }

        if (targetNode is null)
        {
            throw new InvalidOperationException("Failed to parse target document.");
        }

        return Generate(sourceNode, targetNode, info);
    }

    /// <summary>
    /// Generates an overlay document by loading two OpenAPI documents from file paths or URIs.
    /// </summary>
    /// <param name="sourcePath">Path or URI to the source document.</param>
    /// <param name="targetPath">Path or URI to the target document.</param>
    /// <returns>A ReadResult containing the overlay document and any diagnostics.</returns>
    public static Task<ReadResult> GenerateAsync(
        string sourcePath,
        string targetPath)
    {
        return GenerateAsync(sourcePath, targetPath, null, null, null, CancellationToken.None);
    }

    /// <summary>
    /// Generates an overlay document by loading two OpenAPI documents from file paths or URIs.
    /// </summary>
    /// <param name="sourcePath">Path or URI to the source document.</param>
    /// <param name="targetPath">Path or URI to the target document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A ReadResult containing the overlay document and any diagnostics.</returns>
    public static Task<ReadResult> GenerateAsync(
        string sourcePath,
        string targetPath,
        CancellationToken cancellationToken)
    {
        return GenerateAsync(sourcePath, targetPath, null, null, null, cancellationToken);
    }

    /// <summary>
    /// Generates an overlay document by loading two OpenAPI documents from file paths or URIs.
    /// </summary>
    /// <param name="sourcePath">Path or URI to the source document.</param>
    /// <param name="targetPath">Path or URI to the target document.</param>
    /// <param name="format">The format of the documents (json or yaml).</param>
    /// <param name="info">Overlay info metadata.</param>
    /// <param name="readerSettings">Reader settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A ReadResult containing the overlay document and any diagnostics.</returns>
    public static async Task<ReadResult> GenerateAsync(
        string sourcePath,
        string targetPath,
        string? format,
        OverlayInfo? info,
        OverlayReaderSettings? readerSettings,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourcePath);
        ArgumentException.ThrowIfNullOrEmpty(targetPath);

        readerSettings ??= new OverlayReaderSettings();

        Stream sourceStream;
        if (sourcePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            sourcePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            using var response = await readerSettings.HttpClient.GetAsync(sourcePath, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        }

        Stream targetStream;
        if (targetPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            targetPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            using var response = await readerSettings.HttpClient.GetAsync(targetPath, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            targetStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            targetStream = new FileStream(targetPath, FileMode.Open, FileAccess.Read);
        }

        try
        {
            return await GenerateFromStreamsAsync(sourceStream, targetStream, format, info, readerSettings, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await sourceStream.DisposeAsync().ConfigureAwait(false);
            await targetStream.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static bool ValidateOpenApiVersions(JsonNode sourceDocument, JsonNode targetDocument, OverlayDiagnostic diagnostic)
    {
        var sourceVersion = ExtractOpenApiVersion(sourceDocument);
        var targetVersion = ExtractOpenApiVersion(targetDocument);

        if (sourceVersion is null && targetVersion is null)
        {
            return true;
        }

        if (sourceVersion is null || targetVersion is null)
        {
            diagnostic.Errors.Add(new OpenApiError(
                "#/",
                "One or both documents are missing the 'openapi' version property."));
            return false;
        }

        if (!string.Equals(sourceVersion, targetVersion, StringComparison.Ordinal))
        {
            diagnostic.Errors.Add(new OpenApiError(
                "#/",
                $"The OpenAPI versions do not match. Source version: '{sourceVersion}', Target version: '{targetVersion}'."));
            return false;
        }

        return true;
    }

    private static string? ExtractOpenApiVersion(JsonNode document)
    {
        if (document is not JsonObject jsonObject)
        {
            return null;
        }

        return jsonObject["openapi"]?.GetValue<string>();
    }

    private static void GenerateDiff(JsonNode source, JsonNode target, string path, List<OverlayAction> actions)
    {
        if (source is JsonObject sourceObject && target is JsonObject targetObject)
        {
            // Find removed properties
            foreach (var sourceProp in sourceObject)
            {
                if (!targetObject.ContainsKey(sourceProp.Key))
                {
                    var propPath = BuildJsonPath(path, sourceProp.Key);
                    actions.Add(new OverlayAction
                    {
                        Target = propPath,
                        Description = $"Remove property '{sourceProp.Key}'",
                        Remove = true
                    });
                }
            }

            // Group additions and modifications
            var addedProperties = new Dictionary<string, JsonNode>();
            var updatedProperties = new Dictionary<string, JsonNode>();
            var modifiedProperties = new List<(string Key, JsonNode SourceValue, JsonNode TargetValue)>();
            var arrayChanges = new List<(string Key, JsonArray SourceArray, JsonArray TargetArray)>();

            // Find added or modified properties
            foreach (var targetProp in targetObject)
            {
                if (!sourceObject.ContainsKey(targetProp.Key))
                {
                    // Property added
                    if (targetProp.Value is not null)
                    {
                        addedProperties[targetProp.Key] = targetProp.Value;
                    }
                }
                else
                {
                    // Property exists in both, check for differences
                    var sourceValue = sourceObject[targetProp.Key];
                    var targetValue = targetProp.Value;

                    if (sourceValue is not null && targetValue is not null)
                    {
                        if (!JsonNode.DeepEquals(sourceValue, targetValue))
                        {
                            // Check if both are objects with nested changes
                            if (sourceValue is JsonObject srcObj && targetValue is JsonObject tgtObj)
                            {
                                // Check if this object contains array changes that need special handling
                                var hasNestedArrayChanges = HasNestedArrayChanges(srcObj, tgtObj, out var nestedArrayPaths);
                                
                                // Check if this is adding properties to an existing object
                                var hasAddedProps = false;
                                foreach (var prop in tgtObj)
                                {
                                    if (!srcObj.ContainsKey(prop.Key))
                                    {
                                        hasAddedProps = true;
                                        break;
                                    }
                                }

                                if (hasAddedProps || hasNestedArrayChanges)
                                {
                                    // This object has new properties added or nested array changes
                                    // We'll update the whole object, but first handle nested array changes
                                    if (hasNestedArrayChanges)
                                    {
                                        // Generate remove actions for each nested array before updating the parent
                                        var objPath = BuildJsonPath(path, targetProp.Key);
                                        foreach (var arrayPath in nestedArrayPaths)
                                        {
                                            // Build the full path for the nested array using proper JSONPath syntax
                                            // arrayPath might be like "get.parameters" so we need to split and build properly
                                            var pathSegments = arrayPath.Split('.');
                                            var currentPath = objPath;
                                            foreach (var segment in pathSegments)
                                            {
                                                currentPath = BuildJsonPath(currentPath, segment);
                                            }
                                            actions.Add(new OverlayAction
                                            {
                                                Target = currentPath,
                                                Description = $"Remove array at '{arrayPath}'",
                                                Remove = true
                                            });
                                        }
                                    }
                                    
                                    updatedProperties[targetProp.Key] = targetValue;
                                }
                                else
                                {
                                    // Just modifications, recurse
                                    modifiedProperties.Add((targetProp.Key, sourceValue, targetValue));
                                }
                            }
                            else
                            {
                                modifiedProperties.Add((targetProp.Key, sourceValue, targetValue));
                            }
                        }
                    }
                    else if (sourceValue is null && targetValue is not null)
                    {
                        addedProperties[targetProp.Key] = targetValue;
                    }
                    else if (sourceValue is not null && targetValue is null)
                    {
                        var propPath = BuildJsonPath(path, targetProp.Key);
                        actions.Add(new OverlayAction
                        {
                            Target = propPath,
                            Description = $"Remove property '{targetProp.Key}'",
                            Remove = true
                        });
                    }
                }
            }

            // Combine added and updated properties into one action if there are multiple
            var allNewOrUpdated = addedProperties.Concat(updatedProperties).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            
            if (allNewOrUpdated.Count > 1)
            {
                var updateObject = new JsonObject();
                foreach (var kvp in allNewOrUpdated)
                {
                    updateObject[kvp.Key] = kvp.Value.DeepClone();
                }

                var addedKeys = addedProperties.Keys.ToList();
                var updatedKeys = updatedProperties.Keys.ToList();
                var description = new List<string>();
                if (addedKeys.Any())
                {
                    description.Add($"add {string.Join(", ", addedKeys.Select(k => $"'{k}'"))}");
                }
                if (updatedKeys.Any())
                {
                    description.Add($"update {string.Join(", ", updatedKeys.Select(k => $"'{k}'"))}");
                }

                actions.Add(new OverlayAction
                {
                    Target = path,
                    Description = $"Update properties: {string.Join(" and ", description)}",
                    Update = updateObject
                });
            }
            else if (allNewOrUpdated.Count == 1)
            {
                var kvp = allNewOrUpdated.First();
                var isAdded = addedProperties.ContainsKey(kvp.Key);
                var verb = isAdded ? "Add" : "Update";
                
                actions.Add(new OverlayAction
                {
                    Target = path,
                    Description = $"{verb} property '{kvp.Key}'",
                    Update = new JsonObject { [kvp.Key] = kvp.Value.DeepClone() }
                });
            }

            // Process modifications that need recursion
            foreach (var (key, sourceValue, targetValue) in modifiedProperties)
            {
                var propPath = BuildJsonPath(path, key);

                // Values differ
                if (sourceValue is JsonObject && targetValue is JsonObject)
                {
                    // Recurse into objects
                    GenerateDiff(sourceValue, targetValue, propPath, actions);
                }
                else if (sourceValue is JsonArray && targetValue is JsonArray)
                {
                    // For arrays, generate remove + add to replace completely
                    actions.Add(new OverlayAction
                    {
                        Target = propPath,
                        Description = $"Remove array '{key}'",
                        Remove = true
                    });
                    actions.Add(new OverlayAction
                    {
                        Target = path,
                        Description = $"Add array '{key}'",
                        Update = new JsonObject { [key] = targetValue.DeepClone() }
                    });
                }
                else
                {
                    // Simple value change
                    actions.Add(new OverlayAction
                    {
                        Target = propPath,
                        Description = $"Update property '{key}'",
                        Update = targetValue.DeepClone()
                    });
                }
            }
        }
        else if (source is JsonArray sourceArray && target is JsonArray targetArray)
        {
            // For arrays at the root, generate replacement
            if (!JsonNode.DeepEquals(source, target))
            {
                actions.Add(new OverlayAction
                {
                    Target = path,
                    Description = $"Update array",
                    Update = target.DeepClone()
                });
            }
        }
        else
        {
            // Direct value comparison
            if (!JsonNode.DeepEquals(source, target))
            {
                actions.Add(new OverlayAction
                {
                    Target = path,
                    Description = $"Update value",
                    Update = target.DeepClone()
                });
            }
        }
    }

    /// <summary>
    /// Detects if an object has nested arrays that have changed between source and target.
    /// </summary>
    private static bool HasNestedArrayChanges(JsonObject sourceObj, JsonObject targetObj, out List<string> arrayPaths)
    {
        arrayPaths = new List<string>();
        
        foreach (var targetProp in targetObj)
        {
            if (sourceObj.TryGetPropertyValue(targetProp.Key, out var sourcePropValue) && 
                targetProp.Value is not null)
            {
                if (sourcePropValue is JsonArray sourceArray && targetProp.Value is JsonArray targetArray)
                {
                    // Direct array property that changed
                    if (!JsonNode.DeepEquals(sourceArray, targetArray))
                    {
                        arrayPaths.Add(targetProp.Key);
                    }
                }
                else if (sourcePropValue is JsonObject srcNestedObj && targetProp.Value is JsonObject tgtNestedObj)
                {
                    // Recursively check nested objects for array changes
                    if (HasNestedArrayChanges(srcNestedObj, tgtNestedObj, out var nestedPaths))
                    {
                        // Prepend current property to nested paths
                        foreach (var nestedPath in nestedPaths)
                        {
                            arrayPaths.Add($"{targetProp.Key}.{nestedPath}");
                        }
                    }
                }
            }
        }

        return arrayPaths.Count > 0;
    }

    private static string BuildJsonPath(string basePath, string propertyName)
    {
        if (string.IsNullOrEmpty(basePath) || basePath == "$")
        {
            // Check if property name needs bracket notation
            if (NeedsJsonPathEscaping(propertyName))
            {
                return $"$['{propertyName}']";
            }
            return $"$.{propertyName}";
        }
        
        // Check if property name needs bracket notation
        if (NeedsJsonPathEscaping(propertyName))
        {
            return $"{basePath}['{propertyName}']";
        }
        return $"{basePath}.{propertyName}";
    }

    private static bool NeedsJsonPathEscaping(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return true;
        }

        // Property names that start with special characters or contain certain characters need bracket notation
        return propertyName.StartsWith('/') ||
               propertyName.StartsWith('~') ||
               propertyName.Contains(' ') ||
               propertyName.Contains('.') ||
               propertyName.Contains('[') ||
               propertyName.Contains(']') ||
               propertyName.Contains('\'') ||
               propertyName.Contains('"') ||
               !char.IsLetter(propertyName[0]);
    }
}
