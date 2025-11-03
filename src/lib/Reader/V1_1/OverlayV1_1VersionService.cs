
// Licensed under the MIT license.

using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

/// <summary>
/// The version service for the Overlay 1.1 specification.
/// </summary>
internal class OverlayV1_1VersionService : BaseOverlayVersionService
{
    private readonly Dictionary<Type, Func<ParseNode, object?>> _loaders = new()
    {
        [typeof(JsonNodeExtension)] = OverlayV1Deserializer.LoadAny,
        [typeof(OverlayAction)] = OverlayV1_1Deserializer.LoadAction,
        [typeof(OverlayDocument)] = OverlayV1_1Deserializer.LoadDocument,
        [typeof(OverlayInfo)] = OverlayV1_1Deserializer.LoadInfo,
    };

    protected override Dictionary<Type, Func<ParseNode, object?>> Loaders => _loaders;

    public override OverlayDocument LoadDocument(RootNode rootNode)
    {
        return OverlayV1_1Deserializer.LoadDocument(rootNode.GetMap());
    }
}