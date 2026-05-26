
// Licensed under the MIT license.

using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

/// <summary>
/// The version service for the Overlay 1.0 specification.
/// </summary>
#pragma warning disable BOO002
internal class OverlayV1VersionService : BaseOverlayVersionService
{
    private static readonly Dictionary<Type, Func<JsonNode, ParsingContext, object?>> _loaders = new()
    {
        [typeof(JsonNodeExtension)] = OverlayV1Deserializer.LoadAny,
        [typeof(OverlayAction)] = OverlayV1Deserializer.LoadAction,
        [typeof(OverlayComponents)] = OverlayV1Deserializer.LoadComponents,
        [typeof(OverlayReusableAction)] = OverlayV1Deserializer.LoadReusableAction,
        [typeof(OverlayReusableActionReference)] = OverlayV1Deserializer.LoadReusableActionReference,
        [typeof(OverlayDocument)] = OverlayV1Deserializer.LoadDocument,
        [typeof(OverlayInfo)] = OverlayV1Deserializer.LoadInfo,
    };

    protected override Dictionary<Type, Func<JsonNode, ParsingContext, object?>> Loaders => _loaders;

    public override OverlayDocument LoadDocument(JsonNode jsonNode, ParsingContext context)
    {
        return OverlayV1Deserializer.LoadDocument(jsonNode, context);
    }
}
#pragma warning restore BOO002