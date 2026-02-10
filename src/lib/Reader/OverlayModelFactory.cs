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

    private static Task<ReadResult> InternalLoadAsync(Stream input, string format, OverlayReaderSettings settings, CancellationToken cancellationToken = default)
    {
        settings ??= DefaultReaderSettings.Value;
        var reader = settings.GetReader(format);

        return reader.ReadAsync(input, settings, cancellationToken);
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
        var trimmedInput = input.TrimStart();
        return trimmedInput.StartsWith("{", StringComparison.OrdinalIgnoreCase) || trimmedInput.StartsWith("[", StringComparison.OrdinalIgnoreCase) ?
            OpenApiConstants.Json : OpenApiConstants.Yaml;
    }

    /// <summary>
    /// Reads the initial bytes of the stream to determine if it is JSON or YAML.
    /// </summary>
    /// <remarks>
    /// It is important NOT TO change the stream type from MemoryStream.
    /// In Asp.Net core 3.0+ we could get passed a stream from a request or response body.
    /// In such case, we CAN'T use the ReadByte method as it throws NotSupportedException.
    /// Therefore, we need to ensure that the stream is a MemoryStream before calling this method.
    /// Maintaining this type ensures there won't be any unforeseen wrong usage of the method.
    /// </remarks>
    /// <param name="stream">The stream to inspect</param>
    /// <returns>The format of the stream.</returns>
    private static string InspectStreamFormat(MemoryStream stream)
    {
        return TryInspectStreamFormat(stream, out var format) ? format! : throw new InvalidOperationException("Could not determine the format of the stream.");
    }

    private static bool TryInspectStreamFormat(Stream stream, out string? format)
    {
        ArgumentNullException.ThrowIfNull(stream);

        try
        {
            var initialPosition = stream.Position;
            var firstByte = (char)stream.ReadByte();

            // Skip whitespace if present and read the next non-whitespace byte
            while (char.IsWhiteSpace(firstByte))
            {
                firstByte = (char)stream.ReadByte();
            }

            stream.Position = initialPosition; // Reset the stream position to the beginning

            format = firstByte switch
            {
                '{' or '[' => OpenApiConstants.Json, // If the first character is '{' or '[', assume JSON
                _ => OpenApiConstants.Yaml // Otherwise assume YAML
            };
            return true;
        }
        catch (NotSupportedException)
        {
            // https://github.com/dotnet/aspnetcore/blob/c9d0750396e1d319301255ba61842721ab72ab10/src/Servers/Kestrel/Core/src/Internal/Http/HttpResponseStream.cs#L40
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("AllowSynchronousIO", StringComparison.Ordinal))
        {
            // https://github.com/dotnet/aspnetcore/blob/c9d0750396e1d319301255ba61842721ab72ab10/src/Servers/HttpSys/src/RequestProcessing/RequestStream.cs#L100-L108
            // https://github.com/dotnet/aspnetcore/blob/c9d0750396e1d319301255ba61842721ab72ab10/src/Servers/IIS/IIS/src/Core/HttpRequestStream.cs#L24-L30
            // https://github.com/dotnet/aspnetcore/blob/c9d0750396e1d319301255ba61842721ab72ab10/src/Servers/Kestrel/Core/src/Internal/Http/HttpRequestStream.cs#L54-L60
        }
        format = null;
        return false;
    }

    private static async Task<MemoryStream> CopyToMemoryStreamAsync(Stream input, CancellationToken token)
    {
        var bufferStream = new MemoryStream();
        await input.CopyToAsync(bufferStream, 81920, token).ConfigureAwait(false);
        bufferStream.Position = 0;
        return bufferStream;
    }

    private static async Task<(Stream, string)> PrepareStreamForReadingAsync(Stream input, string? format = null, CancellationToken token = default)
    {
        Stream preparedStream = input;

        if (input is MemoryStream ms)
        {
            format ??= InspectStreamFormat(ms);
        }
        else if (!input.CanSeek || !TryInspectStreamFormat(input, out format!))
        {
            // Copy to a MemoryStream to enable seeking and perform format inspection
            var bufferStream = await CopyToMemoryStreamAsync(input, token).ConfigureAwait(false);
            return await PrepareStreamForReadingAsync(bufferStream, format, token).ConfigureAwait(false);
        }

        return (preparedStream, format);
    }
}