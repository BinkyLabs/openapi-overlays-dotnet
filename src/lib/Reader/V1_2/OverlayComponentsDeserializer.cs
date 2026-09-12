using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader.V1;
using BinkyLabs.OpenApi.Overlays.Reader.V1_1;

namespace BinkyLabs.OpenApi.Overlays.Reader.V1_2;

internal static partial class OverlayV1_2Deserializer
{
    public static readonly FixedFieldMap<OverlayComponents> ComponentsFixedFields = new(
        OverlayV1_1Deserializer.ComponentsFixedFields,
        [OverlayConstants.ComponentsActionsFieldName])
    {
        { OverlayConstants.ComponentsActionsFieldName, (o, v, c) => o.Actions = v.CreateMap<OverlayReusableAction>(LoadReusableAction, c) }
    };

    public static readonly PatternFieldMap<OverlayComponents> ComponentsPatternFields = new();

    public static OverlayComponents LoadComponents(JsonNode node, ParsingContext context) =>
        OverlayV1Deserializer.LoadComponentsInternal(node, context, ComponentsFixedFields, ComponentsPatternFields);
}