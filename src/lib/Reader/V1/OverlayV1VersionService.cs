
// Licensed under the MIT license.

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

/// <summary>
/// The version service for the Overlay 1.0 specification.
/// </summary>
internal class OverlayV1VersionService : BaseOverlayVersionService
{
    private static readonly Dictionary<Type, Func<ParseNode, object?>> _loaders = new()
    {
        [typeof(JsonNodeExtension)] = OverlayV1Deserializer.LoadAny,
        [typeof(OverlayAction)] = OverlayV1Deserializer.LoadAction,
        [typeof(OverlayDocument)] = OverlayV1Deserializer.LoadDocument,
        [typeof(OverlayInfo)] = OverlayV1Deserializer.LoadInfo,
    };

    protected override Dictionary<Type, Func<ParseNode, object?>> Loaders => _loaders;

    public override OverlayDocument LoadDocument(RootNode rootNode)
    {
        return OverlayV1Deserializer.LoadDocument(rootNode.GetMap());
    }
}