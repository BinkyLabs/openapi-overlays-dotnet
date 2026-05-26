using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayInfo> InfoFixedFields = new()
    {
        { OverlayConstants.InfoTitleFieldName, (o, v, _) => o.Title = v.GetScalarValue() },
        { OverlayConstants.InfoVersionFieldName, (o, v, _) => o.Version = v.GetScalarValue() },
        { OverlayConstants.InfoXDescriptionFieldName, (o, v, _) => o.Description = v.GetScalarValue() }
    };
    public static PatternFieldMap<OverlayInfo> GetInfoPatternFields(OverlaySpecVersion version) =>
    new()
    {
        {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n, c) => o.AddExtension(k, LoadExtension(k, n, version, c))}
    };
    public static readonly PatternFieldMap<OverlayInfo> InfoPatternFields = GetInfoPatternFields(OverlaySpecVersion.Overlay1_0);
    public static OverlayInfo LoadInfo(JsonNode node, ParsingContext context) => LoadInfoInternal(node, context, InfoFixedFields, InfoPatternFields);
    public static OverlayInfo LoadInfoInternal(JsonNode node, ParsingContext context, FixedFieldMap<OverlayInfo> infoFixedFields, PatternFieldMap<OverlayInfo> infoPatternFields)
    {
        var mapNode = node.CheckMapNode("Info", context);
        var info = new OverlayInfo();
        ParseMap(mapNode, info, infoFixedFields, infoPatternFields, context);

        return info;
    }
}