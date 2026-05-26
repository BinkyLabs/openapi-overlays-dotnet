using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_1;

#pragma warning disable BOO002
internal static partial class OverlayV1_1Deserializer
{
    public static readonly FixedFieldMap<OverlayComponents> ComponentsFixedFields =
        new(OverlayV1Deserializer.ComponentsFixedFields);

    public static readonly PatternFieldMap<OverlayComponents> ComponentsPatternFields = new();

    public static OverlayComponents LoadComponents(JsonNode node, ParsingContext context) =>
        OverlayV1Deserializer.LoadComponentsInternal(node, context, ComponentsFixedFields, ComponentsPatternFields);
}
#pragma warning restore BOO002