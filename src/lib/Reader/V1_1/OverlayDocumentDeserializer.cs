using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayDocument> DocumentFixedFields = new(OverlayV1Deserializer.DocumentFixedFields, ["info", "actions"])
    {
        { "info", (o, v) => o.Info = LoadInfo(v) },
        { "actions", (o, v) => o.Actions = v.CreateList<OverlayAction>(LoadAction) }
    };
    public static readonly PatternFieldMap<OverlayDocument> DocumentPatternFields = OverlayV1Deserializer.GetDocumentPatternFields(OverlaySpecVersion.Overlay1_1);
    public static OverlayDocument LoadDocument(ParseNode node) => OverlayV1Deserializer.LoadDocumentInternal(node, DocumentFixedFields, DocumentPatternFields);
}