// Licensed under the MIT license.

using System.Text.Json.Nodes;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays.Tests.Reader;

public class OverlayReaderSettingsTests
{
    [Fact]
    public void Default_HttpClient_IsLazyShared()
    {
        var settings = new OverlayReaderSettings();
        Assert.NotNull(settings.HttpClient);
        Assert.Same(settings.HttpClient, settings.HttpClient);
    }

    [Fact]
    public void HttpClient_CanBeInitialized()
    {
        using var http = new HttpClient();
        var settings = new OverlayReaderSettings { HttpClient = http };
        Assert.Same(http, settings.HttpClient);
    }

    [Fact]
    public void Readers_DefaultHasJsonAndYaml()
    {
        var settings = new OverlayReaderSettings();
        Assert.True(settings.Readers.ContainsKey(OpenApiConstants.Json));
        Assert.True(settings.Readers.ContainsKey(OpenApiConstants.Yaml));
    }

    [Fact]
    public void Readers_Init_CopiesNonOrdinalIgnoreCaseDictionary()
    {
        var dict = new Dictionary<string, IOverlayReader>(StringComparer.Ordinal)
        {
            { "json", new OverlayJsonReader() }
        };
        var settings = new OverlayReaderSettings { Readers = dict };

        Assert.True(settings.Readers.ContainsKey("JSON"));
    }

    [Fact]
    public void Readers_Init_KeepsOrdinalIgnoreCaseDictionary()
    {
        var dict = new Dictionary<string, IOverlayReader>(StringComparer.OrdinalIgnoreCase)
        {
            { "json", new OverlayJsonReader() }
        };
        var settings = new OverlayReaderSettings { Readers = dict };

        Assert.Same(dict, settings.Readers);
    }

    [Fact]
    public void Readers_Init_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => new OverlayReaderSettings { Readers = null! });
    }

    [Fact]
    public void TryAddReader_AddsNewFormat()
    {
        var settings = new OverlayReaderSettings();
        var added = settings.TryAddReader("xml", new OverlayJsonReader());
        Assert.True(added);
        Assert.True(settings.Readers.ContainsKey("xml"));
    }

    [Fact]
    public void TryAddReader_ReturnsFalseWhenExists()
    {
        var settings = new OverlayReaderSettings();
        var added = settings.TryAddReader(OpenApiConstants.Json, new OverlayJsonReader());
        Assert.False(added);
    }

    [Fact]
    public void TryAddReader_ThrowsOnNullArgs()
    {
        var settings = new OverlayReaderSettings();
        Assert.Throws<ArgumentException>(() => settings.TryAddReader("", new OverlayJsonReader()));
        Assert.Throws<ArgumentNullException>(() => settings.TryAddReader("xml", null!));
    }

    [Fact]
    public void AddJsonReader_NoOpWhenAlreadyPresent()
    {
        var settings = new OverlayReaderSettings();
        settings.AddJsonReader();
        Assert.True(settings.Readers.ContainsKey(OpenApiConstants.Json));
    }

    [Fact]
    public void GetReader_ReturnsRegisteredReader()
    {
        var settings = new OverlayReaderSettings();
        var reader = (IOverlayReader)typeof(OverlayReaderSettings)
            .GetMethod("GetReader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(settings, [OpenApiConstants.Json])!;
        Assert.NotNull(reader);
    }

    [Fact]
    public void GetReader_ThrowsForUnknownFormat()
    {
        var settings = new OverlayReaderSettings();
        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() => typeof(OverlayReaderSettings)
            .GetMethod("GetReader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(settings, ["unknown-format"]));
        Assert.IsType<NotSupportedException>(ex.InnerException);
    }

    [Fact]
    public void GetReader_ThrowsOnEmptyFormat()
    {
        var settings = new OverlayReaderSettings();
        var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() => typeof(OverlayReaderSettings)
            .GetMethod("GetReader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(settings, [""]));
        Assert.IsType<ArgumentException>(ex.InnerException);
    }

    [Fact]
    public void ExtensionParsers_CanBeAssigned()
    {
        var settings = new OverlayReaderSettings
        {
            ExtensionParsers = new Dictionary<string, Func<JsonNode, OverlaySpecVersion, IOverlayExtension>>
            {
                ["x-foo"] = (n, _) => new JsonNodeExtension(n)
            }
        };
        Assert.NotNull(settings.ExtensionParsers);
        Assert.True(settings.ExtensionParsers!.ContainsKey("x-foo"));
    }

    [Fact]
    public void OpenApiSettings_IsSettable()
    {
        var settings = new OverlayReaderSettings { OpenApiSettings = new OpenApiReaderSettings { BaseUrl = new Uri("https://example.com/") } };
        Assert.Equal(new Uri("https://example.com/"), settings.OpenApiSettings.BaseUrl);
    }
}