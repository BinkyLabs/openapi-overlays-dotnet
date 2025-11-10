namespace BinkyLabs.OpenApi.Overlays.Reader.V1;

#pragma warning disable BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

internal static partial class OverlayV1Deserializer
{
    public static readonly FixedFieldMap<OverlayParameter> ParameterFixedFields = new()
    {
        { "name", (o, v) => o.Name = v.GetScalarValue() },
        { "defaultValues", (o, v) =>
            {
                if (v is ListNode listNode)
                {
                    o.DefaultValues = [];
                    foreach (var item in listNode)
                    {
                        var itemValue = item.GetScalarValue();
                        if (!string.IsNullOrEmpty(itemValue))
                        {
                            o.DefaultValues.Add(itemValue);
                        }
                    }
                }
            }
        },
        { "separator", (o, v) => o.Separator = v.GetScalarValue() },
    };

    public static readonly PatternFieldMap<OverlayParameter> ParameterPatternFields = new()
    {
        // Parameters don't have extension fields
    };

    public static OverlayParameter LoadParameter(ParseNode node)
    {
        var mapNode = node.CheckMapNode("Parameter");
        var parameter = new OverlayParameter();
        ParseMap(mapNode, parameter, ParameterFixedFields, ParameterPatternFields);

        return parameter;
    }
}

#pragma warning restore BOO002 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.