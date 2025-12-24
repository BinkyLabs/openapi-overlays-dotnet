// Licensed under the MIT license.

using System.Security;
using System.Text;

using BinkyLabs.OpenApi.Overlays;
using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;


namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// A factory class for loading Overlay models from various sources.
/// </summary>
public static class OverlayModelFactory
{
    /// <summary>
    /// Loads the input URL and parses it into an Open API document.
    /// </summary>
    /// <param name="url">The path to the Overlay file</param>
    /// <param name="settings"> The Overlay reader settings.</param>
    /// <param name="token">The cancellation token</param>
    /// <returns></returns>
    public static async Task<ReadResult> LoadFormUrlAsync(string url,
                                                   OverlayReaderSettings? settings = null,
                                                   CancellationToken token = default)
    {
        settings ??= DefaultReaderSettings.Value;
        var (stream, format) = await RetrieveStreamAndFormatAsync(url, settings, token).ConfigureAwait(false);
        using (stream)
        {
            return await LoadFromStreamAsync(stream, format, settings, token).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Loads the input stream and parses it into an Open API document.  If the stream is not buffered and it contains yaml, it will be buffered before parsing.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <param name="settings"> The Overlay reader settings.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
    /// <param name="format">The Open API format</param>
    /// <returns></returns>
    public static async Task<ReadResult> LoadFromStreamAsync(Stream input, string? format = null, OverlayReaderSettings? settings = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        settings ??= new OverlayReaderSettings();

        Stream? preparedStream = null;
        if (format is null)
        {
            (preparedStream, format) = await PrepareStreamForReadingAsync(input, format, cancellationToken).ConfigureAwait(false);
        }

        // Use StreamReader to process the prepared stream (buffered for YAML, direct for JSON)
        var result = await InternalLoadAsync(preparedStream ?? input, format, settings, cancellationToken).ConfigureAwait(false);

        if (preparedStream is not null && preparedStream != input)
        {

            await preparedStream.DisposeAsync().ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Reads the input string and parses it into an Open API document.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="format">The Open API format</param>
    /// <param name="settings">The Overlay reader settings.</param>
    /// <returns>An Overlay document instance.</returns>
    public static async Task<ReadResult> ParseAsync(string input,
                                   string? format = null,
                                   OverlayReaderSettings? settings = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(input);

        format ??= InspectInputFormat(input);
        settings ??= new OverlayReaderSettings();

        // Copy string into MemoryStream
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));

        return await InternalLoadAsync(stream, format, settings);
    }

    private static readonly Lazy<OverlayReaderSettings> DefaultReaderSettings = new(() => new OverlayReaderSettings());

    private static async Task<ReadResult> InternalLoadAsync(Stream input, string format, OverlayReaderSettings settings, CancellationToken cancellationToken = default)
    {
        settings ??= DefaultReaderSettings.Value;
        var reader = settings.GetReader(format);

        // Handle URI creation more safely for file paths
        Uri location;
        if (input is FileStream fileStream)
        {
            // Convert to absolute path and then create a file URI to handle relative paths correctly
            var absolutePath = Path.GetFullPath(fileStream.Name);
            location = new Uri(absolutePath, UriKind.Absolute);
        }
        else
        {
            location = new Uri(OpenApiConstants.BaseRegistryUri);
        }

        var readResult = await reader.ReadAsync(input, location, settings, cancellationToken).ConfigureAwait(false);

        return readResult;
    }

    private static async Task<(Stream, string?)> RetrieveStreamAndFormatAsync(string url, OverlayReaderSettings settings, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException($"Parameter {nameof(url)} is null or empty. Please provide the correct path or URL to the file.");
        }
        else
        {
            Stream stream;
            string? format;

            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                var response = await settings.HttpClient.GetAsync(url, token).ConfigureAwait(false);
                var mediaType = response.Content.Headers.ContentType?.MediaType;
                var contentType = mediaType?.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                format = contentType?.Split('/').Last().Split('+').Last().Split('-').Last();

                // for non-standard MIME types e.g. text/x-yaml used in older libs or apps

                stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);

                return (stream, format);
            }
            else
            {
                format = Path.GetExtension(url).Split('.').LastOrDefault();

                try
                {
                    var fileInput = new FileInfo(url);
                    stream = fileInput.OpenRead();
                }
                catch (Exception ex) when (
                    ex is
                        FileNotFoundException or
                        PathTooLongException or
                        DirectoryNotFoundException or
                        IOException or
                        UnauthorizedAccessException or
                        SecurityException or
                        NotSupportedException)
                {
                    throw new InvalidOperationException($"Could not open the file at {url}", ex);
                }

                return (stream, format);
            }
        }
    }

    private static string InspectInputFormat(string input)
    {
        return input.StartsWith("{", StringComparison.OrdinalIgnoreCase) || input.StartsWith("[", StringComparison.OrdinalIgnoreCase) ? OpenApiConstants.Json : OpenApiConstants.Yaml;
    }

    private static string InspectStreamFormat(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        long initialPosition = stream.Position;
        int firstByte = stream.ReadByte();

        // Skip whitespace if present and read the next non-whitespace byte
        if (char.IsWhiteSpace((char)firstByte))
        {
            firstByte = stream.ReadByte();
        }

        stream.Position = initialPosition; // Reset the stream position to the beginning

        char firstChar = (char)firstByte;
        return firstChar switch
        {
            '{' or '[' => OpenApiConstants.Json,  // If the first character is '{' or '[', assume JSON
            _ => OpenApiConstants.Yaml             // Otherwise assume YAML
        };
    }

    private static async Task<(Stream, string)> PrepareStreamForReadingAsync(Stream input, string? format = null, CancellationToken token = default)
    {
        Stream preparedStream = input;

        if (!input.CanSeek)
        {
            // Use a temporary buffer to read a small portion for format detection
            using var bufferStream = new MemoryStream();
            await input.CopyToAsync(bufferStream, 1024, token).ConfigureAwait(false);
            bufferStream.Position = 0;

            // Inspect the format from the buffered portion
            format ??= InspectStreamFormat(bufferStream);

            // If format is JSON, no need to buffer further — use the original stream.
            if (format.Equals(OpenApiConstants.Json, StringComparison.OrdinalIgnoreCase))
            {
                preparedStream = input;
            }
            else
            {
                // YAML or other non-JSON format; copy remaining input to a new stream.
                preparedStream = new MemoryStream();
                bufferStream.Position = 0;
                await bufferStream.CopyToAsync(preparedStream, 81920, token).ConfigureAwait(false); // Copy buffered portion
                await input.CopyToAsync(preparedStream, 81920, token).ConfigureAwait(false); // Copy remaining data
                preparedStream.Position = 0;
            }
        }
        else
        {
            format ??= InspectStreamFormat(input);

            if (!format.Equals(OpenApiConstants.Json, StringComparison.OrdinalIgnoreCase))
            {
                // Buffer stream for non-JSON formats (e.g., YAML) since they require synchronous reading
                preparedStream = new MemoryStream();
                await input.CopyToAsync(preparedStream, 81920, token).ConfigureAwait(false);
                preparedStream.Position = 0;
            }
        }

        return (preparedStream, format);
    }
}