using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_2;

/// <summary>
/// The version service for the Overlay 1.2 specification.
/// </summary>
internal class OverlayV1_2VersionService : BaseOverlayVersionService
{
    private static readonly Dictionary<Type, Func<JsonNode, ParsingContext, object?>> _loaders = new()
    {
        [typeof(JsonNodeExtension)] = OverlayV1Deserializer.LoadAny,
        [typeof(OverlayAction)] = OverlayV1_2Deserializer.LoadAction,
        [typeof(OverlayComponents)] = OverlayV1_2Deserializer.LoadComponents,
        [typeof(OverlayReusableAction)] = OverlayV1_2Deserializer.LoadReusableAction,
        [typeof(OverlayReusableActionReference)] = OverlayV1_2Deserializer.LoadReusableActionReference,
        [typeof(OverlayDocument)] = OverlayV1_2Deserializer.LoadDocument,
        [typeof(OverlayInfo)] = OverlayV1_2Deserializer.LoadInfo,
    };

    protected override Dictionary<Type, Func<JsonNode, ParsingContext, object?>> Loaders => _loaders;

    public override OverlayDocument LoadDocument(JsonNode jsonNode, ParsingContext context)
    {
        return OverlayV1_2Deserializer.LoadDocument(jsonNode, context);
    }
}