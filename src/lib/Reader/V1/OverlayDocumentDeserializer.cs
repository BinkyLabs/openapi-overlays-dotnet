using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayDocument> DocumentFixedFields = new()
    {
        { OverlayConstants.DocumentOverlayFieldName, (o, v, _) => o.Overlay = v.GetScalarValue() },
        { OverlayConstants.DocumentExtendsFieldName, (o, v, c) => o.Extends = LoadDocumentUri(v, c, OverlayConstants.DocumentExtendsFieldName) },
        { OverlayConstants.DocumentInfoFieldName, (o, v, c) => o.Info = LoadInfo(v, c) },
        { OverlayConstants.DocumentActionsFieldName, (o, v, c) => o.Actions = v.CreateList<IOverlayAction>(LoadActionOrReference, c) },
        { OverlayConstants.DocumentXComponentsFieldName, (o, v, c) => o.Components = LoadComponents(v, c) }
    };
    public static PatternFieldMap<OverlayDocument> GetDocumentPatternFields(OverlaySpecVersion version) =>
    new()
    {
        {s => s.StartsWith(OverlayConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase), (o, k, n, c) => o.AddExtension(k, LoadExtension(k, n, version, c))}
    };
    public static readonly PatternFieldMap<OverlayDocument> DocumentPatternFields = GetDocumentPatternFields(OverlaySpecVersion.Overlay1_0);
    public static OverlayDocument LoadDocument(JsonNode node, ParsingContext context) => LoadDocumentInternal(node, context, DocumentFixedFields, DocumentPatternFields);
    public static OverlayDocument LoadDocumentInternal(JsonNode node, ParsingContext context, FixedFieldMap<OverlayDocument> documentFixedFields, PatternFieldMap<OverlayDocument> documentPatternFields)
    {
        var mapNode = node.CheckMapNode("Document", context);
        var document = new OverlayDocument();
        ParseMap(mapNode, document, documentFixedFields, documentPatternFields, context);

        return document;
    }

    internal static Uri? LoadDocumentUri(JsonNode node, ParsingContext context, string fieldName)
    {
        var rawUri = node.GetScalarValue();
        if (rawUri is null || !Uri.TryCreate(rawUri, UriKind.RelativeOrAbsolute, out var result))
        {
            context.Diagnostic.Errors.Add(new OpenApiError(context.GetLocation(), $"The '{fieldName}' property must be a valid URI-reference."));
            return null;
        }

        if (rawUri.Contains('#', StringComparison.Ordinal))
        {
            context.Diagnostic.Errors.Add(new OpenApiError(context.GetLocation(), $"The '{fieldName}' property must not contain a fragment identifier ('#'). Found: '{rawUri}'."));
        }

        return result;
    }

    private static IOverlayAction LoadActionOrReference(JsonNode node, ParsingContext context)
    {
        var mapNode = node.CheckMapNode("Action", context);
        return mapNode[OverlayConstants.ReusableActionReferenceXReferenceFieldName] != null
            ? LoadReusableActionReference(mapNode, context)
            : LoadAction(mapNode, context);
    }
}