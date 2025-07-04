
using System.Net.Http;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays.Reader;
/// <summary>  
/// Represents settings for reading OpenAPI overlays.  
/// </summary>  
public class OverlayReaderSettings
{
    private static readonly Lazy<HttpClient> httpClient = new Lazy<HttpClient>(() => new HttpClient());

    private HttpClient? _httpClient;

    private Dictionary<string, IOpenApiReader> _readers = new(StringComparer.OrdinalIgnoreCase)
        {
            { OpenApiConstants.Json, new OpenApiJsonReader() }
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
    /// Gets or sets the settings for the OpenAPI reader.  
    /// </summary>  
    public OpenApiReaderSettings OpenApiSettings { get; set; } = default!;

    internal IOpenApiReader GetReader(string format)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(format);
        if (Readers.TryGetValue(format, out var reader))
        {
            return reader;
        }

        throw new NotSupportedException($"Format '{format}' is not supported.");
    }

    /// <summary>
    /// Readers to use to parse the OpenAPI document
    /// </summary>
    public Dictionary<string, IOpenApiReader> Readers
    {
        get => _readers;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _readers = value.Comparer is StringComparer stringComparer && stringComparer == StringComparer.OrdinalIgnoreCase ?
                value :
                new Dictionary<string, IOpenApiReader>(value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
