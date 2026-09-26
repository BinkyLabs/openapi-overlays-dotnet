using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_2;

internal static partial class OverlayV1_2Deserializer
{
    public static readonly FixedFieldMap<OverlayInfo> InfoFixedFields =
        new(OverlayV1_1Deserializer.InfoFixedFields);

    public static readonly PatternFieldMap<OverlayInfo> InfoPatternFields =
        OverlayV1Deserializer.GetInfoPatternFields(OverlaySpecVersion.Overlay1_2);

    public static OverlayInfo LoadInfo(JsonNode node, ParsingContext context) =>
        OverlayV1Deserializer.LoadInfoInternal(node, context, InfoFixedFields, InfoPatternFields);
}