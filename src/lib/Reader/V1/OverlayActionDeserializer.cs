using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayAction> ActionFixedFields =
        OverlayAction.GetActionFixedFields(OverlayConstants.ActionXCopyFieldName);

    public static PatternFieldMap<TAction> GetActionPatternFields<TAction>(OverlaySpecVersion version)
        where TAction : IOverlayExtensible =>
        new()
        {
            {
                s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase),
                (o, k, n, c) => o.AddExtension(k, LoadExtension(k, n, version, c))
            }
        };

    public static PatternFieldMap<OverlayAction> GetActionPatternFields(OverlaySpecVersion version) =>
        GetActionPatternFields<OverlayAction>(version);

    public static readonly PatternFieldMap<OverlayAction> ActionPatternFields = GetActionPatternFields(OverlaySpecVersion.Overlay1_0);

    public static OverlayAction LoadAction(JsonNode node, ParsingContext context) =>
        OverlayAction.LoadActionInternal(node, context, ActionFixedFields, ActionPatternFields);

    public static OverlayAction LoadActionInternal(JsonNode node, ParsingContext context, FixedFieldMap<OverlayAction> actionFixedFields, PatternFieldMap<OverlayAction> actionPatternFields)
        => OverlayAction.LoadActionInternal(node, context, actionFixedFields, actionPatternFields);
}