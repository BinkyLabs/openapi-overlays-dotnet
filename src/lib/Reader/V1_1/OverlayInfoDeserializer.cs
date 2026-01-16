using BinkyLabs.OpenApi.Overlays.Reader.V1;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayInfo> InfoFixedFields = new(OverlayV1Deserializer.InfoFixedFields, ["x-description"])
    {
        { "description", (o, v) => o.Description = v.GetScalarValue() }
    };
    public static readonly PatternFieldMap<OverlayInfo> InfoPatternFields = OverlayV1Deserializer.GetInfoPatternFields(OverlaySpecVersion.Overlay1_1);
    public static OverlayInfo LoadInfo(ParseNode node) => OverlayV1Deserializer.LoadInfoInternal(node, InfoFixedFields, InfoPatternFields);
}