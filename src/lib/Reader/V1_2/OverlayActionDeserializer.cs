using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_2;

internal static partial class OverlayV1_2Deserializer
{
    public static readonly FixedFieldMap<OverlayAction> ActionFixedFields =
        new(OverlayV1_1Deserializer.ActionFixedFields);

    public static readonly PatternFieldMap<OverlayAction> ActionPatternFields =
        OverlayV1Deserializer.GetActionPatternFields<OverlayAction>(OverlaySpecVersion.Overlay1_2);

    public static OverlayAction LoadAction(JsonNode node, ParsingContext context) =>
        OverlayAction.LoadActionInternal(node, context, ActionFixedFields, ActionPatternFields);
}