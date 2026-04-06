using System.Text.Json.Nodes;

using BinkyLabs.OpenApi.Overlays.Reader;
using BinkyLabs.OpenApi.Overlays.Reader.V1;
using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents the shared fields and serialization/deserialization behavior for action objects.
/// </summary>
internal sealed class OverlayCommonAction
{
    /// <summary>
    /// REQUIRED. The target of the action (JSON Pointer or similar).
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// The description of the action.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// A boolean value that indicates that the target object or array MUST be removed from the the map or array it is contained in.
    /// The default value is false.
    /// </summary>
    public bool? Remove { get; set; }

    /// <summary>
    /// The update value to be applied to the target.
    /// </summary>
    public JsonNode? Update { get; set; }

    /// <summary>
    /// A string value that indicates that the target object or array MUST be copied to the location indicated by this string, which MUST be a JSON Pointer.
    /// This field is mutually exclusive with the <see cref="Remove"/> and <see cref="Update"/> fields.
    /// </summary>
    public string? Copy { get; set; }

    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }

    public void SerializeAsV1(IOpenApiWriter writer, Action<IOpenApiWriter> serializeAdditionalProperties) =>
        SerializeInternal(writer, OverlaySpecVersion.Overlay1_0, OverlayConstants.ActionXCopyFieldName, serializeAdditionalProperties);

    public void SerializeAsV1_1(IOpenApiWriter writer, Action<IOpenApiWriter> serializeAdditionalProperties) =>
        SerializeInternal(writer, OverlaySpecVersion.Overlay1_1, OverlayConstants.ActionCopyFieldName, serializeAdditionalProperties);

    internal static FixedFieldMap<TAction> GetActionFixedFields<TAction>(
        string copyFieldName,
        Func<TAction, OverlayCommonAction> getCommon)
        where TAction : class
    {
        var fixedFields = new FixedFieldMap<TAction>
        {
            { OverlayConstants.ActionTargetFieldName, (o, v) => getCommon(o).Target = v.GetScalarValue() },
            { OverlayConstants.ActionDescriptionFieldName, (o, v) => getCommon(o).Description = v.GetScalarValue() },
            { OverlayConstants.ActionRemoveFieldName, (o, v) =>
                {
                    if (v.GetScalarValue() is string removeValue && bool.TryParse(removeValue, out var removeBool))
                    {
                        getCommon(o).Remove = removeBool;
                    }
                }
            },
            { OverlayConstants.ActionUpdateFieldName, (o, v) => getCommon(o).Update = v.CreateAny() },
            { copyFieldName, (o, v) => getCommon(o).Copy = v.GetScalarValue() },
        };

        return fixedFields;
    }

    internal static TAction LoadActionInternal<TAction>(
        ParseNode node,
        FixedFieldMap<TAction> actionFixedFields,
        PatternFieldMap<TAction> actionPatternFields)
        where TAction : class, new()
    {
        var mapNode = node.CheckMapNode("Action");
        var action = new TAction();
        OverlayV1Deserializer.ParseMap(mapNode, action, actionFixedFields, actionPatternFields);

        return action;
    }

    private void SerializeInternal(
        IOpenApiWriter writer,
        OverlaySpecVersion version,
        string copyFieldName,
        Action<IOpenApiWriter> serializeAdditionalProperties)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(serializeAdditionalProperties);
        writer.WriteStartObject();
        writer.WriteRequiredProperty(OverlayConstants.ActionTargetFieldName, Target);
        writer.WriteProperty(OverlayConstants.ActionDescriptionFieldName, Description);
        writer.WriteProperty(OverlayConstants.ActionRemoveFieldName, Remove, false);

        if (Update != null)
        {
            writer.WriteOptionalObject(OverlayConstants.ActionUpdateFieldName, Update, static (w, s) => w.WriteAny(s));
        }
        if (Copy != null)
        {
            writer.WriteProperty(copyFieldName, Copy);
        }

        serializeAdditionalProperties(writer);
        writer.WriteOverlayExtensions(Extensions, version);
        writer.WriteEndObject();
    }
}