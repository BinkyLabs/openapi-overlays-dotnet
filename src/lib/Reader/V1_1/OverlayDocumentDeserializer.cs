using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayDocument> DocumentFixedFields = new(OverlayV1Deserializer.DocumentFixedFields, [OverlayConstants.DocumentInfoFieldName, OverlayConstants.DocumentActionsFieldName])
    {
        { OverlayConstants.DocumentInfoFieldName, (o, v) => o.Info = LoadInfo(v) },
        { OverlayConstants.DocumentActionsFieldName, (o, v) => o.Actions = v.CreateList<IOverlayAction>(n => LoadAction(n)) }
    };
    public static readonly PatternFieldMap<OverlayDocument> DocumentPatternFields = OverlayV1Deserializer.GetDocumentPatternFields(OverlaySpecVersion.Overlay1_1);
    public static OverlayDocument LoadDocument(ParseNode node) => OverlayV1Deserializer.LoadDocumentInternal(node, DocumentFixedFields, DocumentPatternFields);
}
