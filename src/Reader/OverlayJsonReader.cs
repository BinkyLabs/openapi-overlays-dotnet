

using System.Text.Json;
using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Reader for OpenAPI overlay documents in JSON format.
/// </summary>
/// <returns></returns>
public class OverlayJsonReader : IOverlayReader
{
    /// <summary>
    /// Parses the JsonNode input into an Open API document.
    /// </summary>
    /// <param name="jsonNode">The JsonNode input.</param>
    /// <param name="location">Location of where the document that is getting loaded is saved</param>
    /// <param name="settings">The Reader settings to be used during parsing.</param>
    /// <returns></returns>
    internal ReadResult Read(JsonNode jsonNode,
                           Uri location,
                           OverlayReaderSettings settings)
    {
        ArgumentNullException.ThrowIfNull(jsonNode);
        ArgumentNullException.ThrowIfNull(settings);

        var diagnostic = new OverlayDiagnostic();
        var context = new ParsingContext(diagnostic)
        {
            ExtensionParsers = settings.ExtensionParsers,
            BaseUrl = settings.OpenApiSettings.BaseUrl,
            DefaultContentType = settings.OpenApiSettings.DefaultContentType
        };

        OverlayDocument? document = null;
        try
        {
            // Parse the OpenAPI Document
            document = context.Parse(jsonNode, location);
        }
        catch (OpenApiException ex)
        {
            diagnostic.Errors.Add(new(ex));
        }

        // Validate the document
        if (document is not null && settings.OpenApiSettings.RuleSet is not null && settings.OpenApiSettings.RuleSet.Rules.Any())
        {
            var openApiErrors = document.Validate(settings.OpenApiSettings.RuleSet);
            if (openApiErrors is not null)
            {
                foreach (var item in openApiErrors.OfType<OpenApiValidatorError>())
                {
                    diagnostic.Errors.Add(item);
                }
                foreach (var item in openApiErrors.OfType<OpenApiValidatorWarning>())
                {
                    diagnostic.Warnings.Add(item);
                }
            }
        }

        return new()
        {
            Document = document,
            Diagnostic = diagnostic
        };
    }

    /// <summary>
    /// Reads the stream input asynchronously and parses it into an Open API document.
    /// </summary>
    /// <param name="input">Memory stream containing OpenAPI description to parse.</param>
    /// <param name="location">Location of where the document that is getting loaded is saved</param>
    /// <param name="settings">The Reader settings to be used during parsing.</param>
    /// <param name="cancellationToken">Propagates notifications that operations should be cancelled.</param>
    /// <returns></returns>
    public async Task<ReadResult> ReadAsync(Stream input,
                                            Uri location,
                                            OverlayReaderSettings settings,
                                            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(settings);

        JsonNode? jsonNode;
        var diagnostic = new OverlayDiagnostic();

        // Parse the JSON text in the stream into JsonNodes
        try
        {
            jsonNode = await JsonNode.ParseAsync(input, cancellationToken: cancellationToken).ConfigureAwait(false) ??
                throw new InvalidOperationException($"failed to parse input stream, {nameof(input)}");
        }
        catch (JsonException ex)
        {
            diagnostic.Errors.Add(new OpenApiError($"#line={ex.LineNumber}", $"Please provide the correct format, {ex.Message}"));
            return new ReadResult
            {
                Document = null,
                Diagnostic = diagnostic
            };
        }

        return Read(jsonNode, location, settings);
    }

    /// <inheritdoc/>
    public async Task<JsonNode?> GetJsonNodeFromStreamAsync(Stream input, CancellationToken cancellationToken = default)
    {
        return await JsonNode.ParseAsync(input, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}