
// Licensed under the MIT license.

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

/// <summary>
/// The version service for the Overlay 1.0 specification.
/// </summary>
#pragma warning disable BOO002
internal class OverlayV1VersionService : BaseOverlayVersionService
{
    private static readonly Dictionary<Type, Func<ParseNode, object?>> _loaders = new()
    {
        [typeof(JsonNodeExtension)] = OverlayV1Deserializer.LoadAny,
        [typeof(OverlayAction)] = OverlayV1Deserializer.LoadAction,
        [typeof(OverlayComponents)] = OverlayV1Deserializer.LoadComponents,
        [typeof(OverlayReusableAction)] = OverlayV1Deserializer.LoadReusableAction,
        [typeof(OverlayDocument)] = OverlayV1Deserializer.LoadDocument,
        [typeof(OverlayInfo)] = OverlayV1Deserializer.LoadInfo,
        [typeof(OverlayReusableActionParameter)] = OverlayV1Deserializer.LoadReusableActionParameter,
    };

    protected override Dictionary<Type, Func<ParseNode, object?>> Loaders => _loaders;

    public override OverlayDocument LoadDocument(RootNode rootNode)
    {
        return OverlayV1Deserializer.LoadDocument(rootNode.GetMap());
    }
}
#pragma warning restore BOO002