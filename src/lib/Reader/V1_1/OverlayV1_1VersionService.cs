
// Licensed under the MIT license.

using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

/// <summary>
/// The version service for the Overlay 1.1 specification.
/// </summary>
#pragma warning disable BOO002
internal class OverlayV1_1VersionService : BaseOverlayVersionService
{
    private static readonly Dictionary<Type, Func<JsonNode, ParsingContext, object?>> _loaders = new()
    {
        [typeof(JsonNodeExtension)] = OverlayV1Deserializer.LoadAny,
        [typeof(OverlayAction)] = OverlayV1_1Deserializer.LoadAction,
        [typeof(OverlayComponents)] = OverlayV1_1Deserializer.LoadComponents,
        [typeof(OverlayReusableAction)] = OverlayV1_1Deserializer.LoadReusableAction,
        [typeof(OverlayReusableActionReference)] = OverlayV1_1Deserializer.LoadReusableActionReference,
        [typeof(OverlayDocument)] = OverlayV1_1Deserializer.LoadDocument,
        [typeof(OverlayInfo)] = OverlayV1_1Deserializer.LoadInfo,
    };

    protected override Dictionary<Type, Func<JsonNode, ParsingContext, object?>> Loaders => _loaders;

    public override OverlayDocument LoadDocument(JsonNode jsonNode, ParsingContext context)
    {
        return OverlayV1_1Deserializer.LoadDocument(jsonNode, context);
    }
}
#pragma warning restore BOO002