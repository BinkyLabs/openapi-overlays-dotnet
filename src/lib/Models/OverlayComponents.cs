using System.Diagnostics.CodeAnalysis;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a Components object as defined in the OpenAPI Overlay specification.
/// See: https://spec.openapis.org/overlay/v1.2.0.html#components-object
/// </summary>
[Experimental("BOO002")]
public class OverlayComponents : IOverlaySerializable
{
    /// <summary>
    /// The reusable actions collection keyed by action name.
    /// </summary>
    public IDictionary<string, OverlayReusableAction>? Actions { get; set; }

    /// <inheritdoc/>
    public void SerializeAsV1(IOpenApiWriter writer) => SerializeInternal(writer, static (w, a) => a.SerializeAsV1(w));

    /// <inheritdoc/>
    public void SerializeAsV1_1(IOpenApiWriter writer) => SerializeInternal(writer, static (w, a) => a.SerializeAsV1_1(w));

    private void SerializeInternal(IOpenApiWriter writer, Action<IOpenApiWriter, OverlayReusableAction> serializeAction)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartObject();
        if (Actions != null)
        {
            writer.WriteOptionalMap(OverlayConstants.ComponentsActionsFieldName, Actions, serializeAction);
        }
        writer.WriteEndObject();
    }
}