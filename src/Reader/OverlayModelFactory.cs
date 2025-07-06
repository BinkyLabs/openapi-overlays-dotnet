// Licensed under the MIT license.

using System.Security;
using System.Text;

using BinkyLabs.OpenApi.Overlays;
using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;


namespace BinkyLabs.Overlay.Overlays;

/// <summary>
/// A factory class for loading Overlay models from various sources.
/// </summary>
public static class OverlayModelFactory
{
    /// <summary>
    /// Loads the input stream and parses it into an Open API document.
    /// </summary>
    /// <param name="stream"> The input stream.</param>
    /// <param name="settings"> The Overlay reader settings.</param>
    /// <param name="format">The Overlay format.</param>
    /// <returns>An Overlay document instance.</returns>
    public static ReadResult Load(MemoryStream stream, string? format = null, OverlayReaderSettings? settings = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        format ??= InspectStreamFormat(stream);
        settings ??= DefaultReaderSettings.Value;

        var result = InternalLoad(stream, format, settings);

        if (!settings.OpenApiSettings.LeaveStreamOpen)
            stream.Dispose();

        return result;
    }

    /// <summary>
    /// Reads the stream input and parses the fragment of an Overlay description into an Open API Element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url">The path to the Overlay file</param>
    /// <param name="version">Version of the Overlay specification that the fragment conforms to.</param>
    /// <param name="settings">The OpenApiReader settings.</param>
    /// <param name="token"></param>
    /// <returns>Instance of newly created IOpenApiElement.</returns>
    /// <returns>The Overlay element.</returns>
    public static async Task<T?> LoadFromUrlAsync<T>(string url,
                                              OverlaySpecVersion version,
                                              OverlayReaderSettings? settings = null,
                                              CancellationToken token = default) where T : IOpenApiElement
    {
        settings ??= DefaultReaderSettings.Value;
        var (stream, format) = await RetrieveStreamAndFormatAsync(url, settings, token).ConfigureAwait(false);
        return await LoadAsync<T>(stream, version, format, settings, token);
    }

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
        return await LoadFromStreamAsync(stream, format, settings, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads the stream input and parses the fragment of an Overlay description into an Open API Element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input">Stream containing Overlay description to parse.</param>
    /// <param name="version">Version of the Overlay specification that the fragment conforms to.</param>
    /// <param name="format"></param>
    /// <param name="diagnostic">Returns diagnostic object containing errors detected during parsing.</param>
    /// <param name="settings">The OpenApiReader settings.</param>
    /// <returns>Instance of newly created IOpenApiElement.</returns>
    /// <returns>The Overlay element.</returns>
    public static T? LoadFromStream<T>(MemoryStream input,
                             OverlaySpecVersion version,
                             string? format,
                             out OverlayDiagnostic diagnostic,
                             OverlayReaderSettings? settings = null) where T : IOpenApiElement
    {
        format ??= InspectStreamFormat(input);
        settings ??= DefaultReaderSettings.Value;
        return settings.GetReader(format).ReadFragment<T>(input, version, out diagnostic, settings);
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
    /// Reads the stream input and ensures it is buffered before passing it to the Load method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="version"></param>
    /// <param name="format"></param>
    /// <param name="settings"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<T?> LoadAsync<T>(Stream input,
                                             OverlaySpecVersion version,
                                             string? format = null,
                                             OverlayReaderSettings? settings = null,
                                             CancellationToken token = default) where T : IOpenApiElement
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input is MemoryStream memoryStream)
        {
            return LoadFromStream<T>(memoryStream, version, format, out var _, settings);
        }
        else
        {
            memoryStream = new MemoryStream();
            await input.CopyToAsync(memoryStream, 81920, token).ConfigureAwait(false);
            memoryStream.Position = 0;
            return LoadFromStream<T>(memoryStream, version, format, out var _, settings);
        }
    }

    /// <summary>
    /// Reads the input string and parses it into an Open API document.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="format">The Open API format</param>
    /// <param name="settings">The Overlay reader settings.</param>
    /// <returns>An Overlay document instance.</returns>
    public static ReadResult Parse(string input,
                                   string? format = null,
                                   OverlayReaderSettings? settings = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(input);

        format ??= InspectInputFormat(input);
        settings ??= new OverlayReaderSettings();

        // Copy string into MemoryStream
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));

        return InternalLoad(stream, format, settings);
    }

    /// <summary>
    /// Reads the input string and parses it into an Open API document.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="version"></param>
    /// <param name="diagnostic">The diagnostic entity containing information from the reading process.</param>
    /// <param name="format">The Open API format</param>
    /// <param name="settings">The Overlay reader settings.</param>
    /// <returns>An Overlay document instance.</returns>
    public static T? Parse<T>(string input,
                             OverlaySpecVersion version,
                             out OverlayDiagnostic diagnostic,
                             string? format = null,
                             OverlayReaderSettings? settings = null) where T : IOpenApiElement
    {
        ArgumentException.ThrowIfNullOrEmpty(input);

        format ??= InspectInputFormat(input);
        settings ??= new OverlayReaderSettings();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        return LoadFromStream<T>(stream, version, format, out diagnostic, settings);
    }

    private static readonly Lazy<OverlayReaderSettings> DefaultReaderSettings = new(() => new OverlayReaderSettings());

    private static async Task<ReadResult> InternalLoadAsync(Stream input, string format, OverlayReaderSettings settings, CancellationToken cancellationToken = default)
    {
        settings ??= DefaultReaderSettings.Value;
        var reader = settings.GetReader(format);
        var location =
                    (input is FileStream fileStream ? new Uri(fileStream.Name) : null) ??
                    new Uri(OpenApiConstants.BaseRegistryUri);

        var readResult = await reader.ReadAsync(input, location, settings, cancellationToken).ConfigureAwait(false);

        return readResult;
    }

    private static ReadResult InternalLoad(MemoryStream input, string format, OverlayReaderSettings settings)
    {
        settings ??= DefaultReaderSettings.Value;

        if (input.Length == 0 || input.Position == input.Length)
        {
            throw new ArgumentException($"Cannot parse the stream: {nameof(input)} is empty or contains no elements.");
        }

        var location = new Uri(OpenApiConstants.BaseRegistryUri);
        var reader = settings.GetReader(format);
        var readResult = reader.Read(input, location, settings);
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