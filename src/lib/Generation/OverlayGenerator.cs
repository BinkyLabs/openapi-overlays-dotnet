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
    /// <returns>An OverlayGenerationResult containing the overlay document and any diagnostics.</returns>
    public static OverlayGenerationResult Generate(JsonNode sourceDocument, JsonNode targetDocument)
    {
        return Generate(sourceDocument, targetDocument, null);
    }

    /// <summary>
    /// Generates an overlay document that represents the differences between two OpenAPI documents.
    /// </summary>
    /// <param name="sourceDocument">The source (original) document as JsonNode.</param>
    /// <param name="targetDocument">The target (modified) document as JsonNode.</param>
    /// <param name="info">Overlay info metadata.</param>
    /// <returns>An OverlayGenerationResult containing the overlay document and any diagnostics.</returns>
    public static OverlayGenerationResult Generate(JsonNode sourceDocument, JsonNode targetDocument, OverlayInfo? info)
    {
        ArgumentNullException.ThrowIfNull(sourceDocument);
        ArgumentNullException.ThrowIfNull(targetDocument);

        var diagnostic = new OverlayDiagnostic();

        if (!ValidateOpenApiVersions(sourceDocument, targetDocument, diagnostic))
        {
            return new OverlayGenerationResult
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

        return new OverlayGenerationResult
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
    /// <returns>An OverlayGenerationResult containing the overlay document and any diagnostics.</returns>
    public static Task<OverlayGenerationResult> GenerateFromStreamsAsync(
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
    /// <returns>An OverlayGenerationResult containing the overlay document and any diagnostics.</returns>
    public static Task<OverlayGenerationResult> GenerateFromStreamsAsync(
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
    /// <returns>An OverlayGenerationResult containing the overlay document and any diagnostics.</returns>
    public static async Task<OverlayGenerationResult> GenerateFromStreamsAsync(
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
    /// <returns>An OverlayGenerationResult containing the overlay document and any diagnostics.</returns>
    public static Task<OverlayGenerationResult> GenerateAsync(
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
    /// <returns>An OverlayGenerationResult containing the overlay document and any diagnostics.</returns>
    public static Task<OverlayGenerationResult> GenerateAsync(
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
    /// <returns>An OverlayGenerationResult containing the overlay document and any diagnostics.</returns>
    public static async Task<OverlayGenerationResult> GenerateAsync(
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
                    var propPath = JsonPathBuilder.BuildPath(path, sourceProp.Key);
                    actions.Add(new OverlayAction
                    {
                        Target = propPath,
                        Description = $"Remove property '{sourceProp.Key}'",
                        Remove = true
                    });
                }
            }

            // Find added or modified properties
            foreach (var targetProp in targetObject)
            {
                var propPath = JsonPathBuilder.BuildPath(path, targetProp.Key);

                if (!sourceObject.ContainsKey(targetProp.Key))
                {
                    // Property added
                    if (targetProp.Value is not null)
                    {
                        actions.Add(new OverlayAction
                        {
                            Target = path,
                            Description = $"Add property '{targetProp.Key}'",
                            Update = new JsonObject { [targetProp.Key] = targetProp.Value.DeepClone() }
                        });
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
                            // Values differ, recurse or generate update
                            if (sourceValue is JsonObject && targetValue is JsonObject)
                            {
                                GenerateDiff(sourceValue, targetValue, propPath, actions);
                            }
                            else if (sourceValue is JsonArray && targetValue is JsonArray)
                            {
                                // For arrays, generate a replacement action
                                actions.Add(new OverlayAction
                                {
                                    Target = propPath,
                                    Description = $"Update array at '{targetProp.Key}'",
                                    Update = targetValue.DeepClone()
                                });
                            }
                            else
                            {
                                // Simple value change
                                actions.Add(new OverlayAction
                                {
                                    Target = propPath,
                                    Description = $"Update property '{targetProp.Key}'",
                                    Update = targetValue.DeepClone()
                                });
                            }
                        }
                    }
                    else if (sourceValue is null && targetValue is not null)
                    {
                        actions.Add(new OverlayAction
                        {
                            Target = path,
                            Description = $"Set property '{targetProp.Key}'",
                            Update = new JsonObject { [targetProp.Key] = targetValue.DeepClone() }
                        });
                    }
                    else if (sourceValue is not null && targetValue is null)
                    {
                        actions.Add(new OverlayAction
                        {
                            Target = propPath,
                            Description = $"Remove property '{targetProp.Key}'",
                            Remove = true
                        });
                    }
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
}
