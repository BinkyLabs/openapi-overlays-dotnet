using Microsoft.OpenApi;

using SharpYaml.Serialization;

namespace BinkyLabs.OpenApi.Overlays.Extensions;

/// <summary>
/// Extension methods for OverlayDocument serialization.
/// </summary>
public static class OverlayDocumentExtensions
{
    /// <summary>
    /// Serializes the overlay document to a stream in the specified format.
    /// </summary>
    /// <param name="document">The overlay document to serialize.</param>
    /// <param name="stream">The output stream.</param>
    /// <param name="version">The overlay specification version.</param>
    /// <param name="format">The output format (json or yaml).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task SerializeAsync(
        this OverlayDocument document,
        Stream stream,
        OverlaySpecVersion version,
        string format,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(format);

        using var textWriter = new StreamWriter(stream, leaveOpen: true);
        
        IOpenApiWriter writer = format.ToLowerInvariant() switch
        {
            "yaml" or "yml" => new OpenApiYamlWriter(textWriter),
            "json" or _ => new OpenApiJsonWriter(textWriter)
        };

        switch (version)
        {
            case OverlaySpecVersion.Overlay1_0:
                document.SerializeAsV1(writer);
                break;
            case OverlaySpecVersion.Overlay1_1:
                document.SerializeAsV1_1(writer);
                break;
            default:
                throw new NotSupportedException($"Overlay version {version} is not supported.");
        }

        await textWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
