using System.Diagnostics.CodeAnalysis;

using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a reusable Action Object as defined in the OpenAPI Overlay specification.
/// See: https://spec.openapis.org/overlay/v1.2.0.html#reusable-action-object
/// </summary>
[Experimental("BOO002")]
public class OverlayReusableAction : IOverlaySerializable, IOverlayExtensible
{
    /// <summary>
    /// A description of the reusable action itself (independent of any description on
    /// <see cref="Fields"/>). CommonMark syntax MAY be used for rich text representation.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The action fields for this reusable action. The <c>target</c> field MUST NOT be set on the
    /// contained <see cref="OverlayAction"/>; it is supplied by the
    /// <see cref="OverlayReusableActionReference"/> that references this reusable action.
    /// </summary>
    public OverlayAction? Fields { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }

    /// <inheritdoc/>
    public void SerializeAsV1(IOpenApiWriter writer) =>
        SerializeInternal(writer, OverlaySpecVersion.Overlay1_0, static (w, f) => f.SerializeFieldsAsV1(w));

    /// <inheritdoc/>
    public void SerializeAsV1_1(IOpenApiWriter writer) =>
        SerializeInternal(writer, OverlaySpecVersion.Overlay1_1, static (w, f) => f.SerializeFieldsAsV1_1(w));

    private void SerializeInternal(
        IOpenApiWriter writer,
        OverlaySpecVersion version,
        Action<IOpenApiWriter, OverlayAction> serializeFields)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartObject();

        if (!string.IsNullOrEmpty(Description))
        {
            writer.WriteProperty(OverlayConstants.ReusableActionDescriptionFieldName, Description);
        }

        writer.WriteRequiredObject(
            OverlayConstants.ReusableActionFieldsFieldName,
            Fields ?? new OverlayAction(),
            serializeFields);

        writer.WriteOverlayExtensions(Extensions, version);
        writer.WriteEndObject();
    }
}