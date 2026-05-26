using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayAction> ActionFixedFields = new(
        OverlayAction.GetActionFixedFields(OverlayConstants.ActionCopyFieldName),
        [OverlayConstants.ActionXCopyFieldName])
    {
    };

    public static readonly PatternFieldMap<OverlayAction> ActionPatternFields =
        OverlayV1Deserializer.GetActionPatternFields<OverlayAction>(OverlaySpecVersion.Overlay1_1);

    public static OverlayAction LoadAction(JsonNode node, ParsingContext context) =>
        OverlayAction.LoadActionInternal(node, context, ActionFixedFields, ActionPatternFields);
}