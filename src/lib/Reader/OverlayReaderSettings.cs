
using System.Text.Json.Nodes;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays;
/// <summary>  
/// Represents settings for reading OpenAPI overlays.  
/// </summary>  
public class OverlayReaderSettings
{
    private static readonly Lazy<HttpClient> httpClient = new Lazy<HttpClient>(() => new HttpClient());

    private HttpClient? _httpClient;

    private Dictionary<string, IOverlayReader> _readers = new(StringComparer.OrdinalIgnoreCase)
        {
            { OpenApiConstants.Json, new OverlayJsonReader() },
            { OpenApiConstants.Yaml, new OverlayYamlReader() }
        };

    /// <summary>  
    /// Gets or initializes the HTTP client used for making requests.  
    /// </summary>  
    public HttpClient HttpClient
    {
        internal get
        {
            if (_httpClient == null)
            {
                _httpClient = httpClient.Value;
            }

            return _httpClient;
        }
        init
        {
            _httpClient = value;
        }
    }

    /// <summary>
    /// Readers to use to parse the OpenAPI document
    /// </summary>
    public Dictionary<string, IOverlayReader> Readers
    {
        get => _readers;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _readers = value.Comparer is StringComparer stringComparer && stringComparer == StringComparer.OrdinalIgnoreCase ?
                value :
                new Dictionary<string, IOverlayReader>(value, StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>  
    /// Gets or sets the settings for the OpenAPI reader.  
    /// </summary>  
    public OpenApiReaderSettings OpenApiSettings { get; set; } = new OpenApiReaderSettings()!;

    /// <summary>
    /// Dictionary of parsers for converting extensions into strongly typed classes
    /// </summary>
    public Dictionary<string, Func<JsonNode, OverlaySpecVersion, IOverlayExtension>>? ExtensionParsers { get; set; } = new();


    internal IOverlayReader GetReader(string format)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(format);
        if (Readers.TryGetValue(format, out var reader))
        {
            return reader;
        }

        throw new NotSupportedException($"Format '{format}' is not supported.");
    }

    /// <summary>
    /// Adds a reader for the specified format
    /// </summary>
    public void AddJsonReader()
    {
        TryAddReader(OpenApiConstants.Json, new OverlayJsonReader());
    }

    /// <summary>
    /// Adds a reader for the specified format.
    /// This method is a no-op if the reader already exists.
    /// This method is equivalent to TryAdd, is provided for compatibility reasons and TryAdd should be used instead when available.
    /// </summary>
    /// <param name="format">Format to add a reader for</param>
    /// <param name="reader">Reader to add</param>
    /// <returns>True if the reader was added, false if it already existed</returns>
    public bool TryAddReader(string format, IOverlayReader reader)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(format);
        ArgumentNullException.ThrowIfNull(reader);
        return Readers.TryAdd(format, reader);
    }


    private void TryAddExtensionParser(string name, Func<JsonNode, OverlaySpecVersion, IOverlayExtension> parser)
    {
        ExtensionParsers?.TryAdd(name, parser);

    }
}