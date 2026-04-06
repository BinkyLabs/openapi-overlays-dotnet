using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayAction> ActionFixedFields =
        OverlayCommonAction.GetActionFixedFields<OverlayAction>(
            OverlayConstants.ActionXCopyFieldName,
            static a => a.CommonAction);

    public static PatternFieldMap<TAction> GetActionPatternFields<TAction>(OverlaySpecVersion version)
        where TAction : IOverlayExtensible =>
        new()
        {
            {
                s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase),
                (o, k, n) => o.AddExtension(k, LoadExtension(k, n, version))
            }
        };

    public static PatternFieldMap<OverlayAction> GetActionPatternFields(OverlaySpecVersion version) =>
        GetActionPatternFields<OverlayAction>(version);

    public static readonly PatternFieldMap<OverlayAction> ActionPatternFields = GetActionPatternFields(OverlaySpecVersion.Overlay1_0);

    public static OverlayAction LoadAction(ParseNode node) =>
        OverlayCommonAction.LoadActionInternal(node, ActionFixedFields, ActionPatternFields);

    public static OverlayAction LoadActionInternal(ParseNode node, FixedFieldMap<OverlayAction> actionFixedFields, PatternFieldMap<OverlayAction> actionPatternFields)
        => OverlayCommonAction.LoadActionInternal(node, actionFixedFields, actionPatternFields);
}