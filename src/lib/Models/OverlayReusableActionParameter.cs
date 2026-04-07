using System.Diagnostics.CodeAnalysis;

using BinkyLabs.OpenApi.Overlays.Writers;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents a reusable action parameter object in the OpenAPI Overlay specification.
/// See: https://spec.openapis.org/overlay/v1.2.0.html#reusable-action-parameter-object
/// </summary>
[Experimental("BOO002")]
public class OverlayReusableActionParameter : IOverlaySerializable, IOverlayExtensible
{
    /// <summary>
    /// REQUIRED. The parameter name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The default value for the parameter.
    /// </summary>
    public string? Default { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, IOverlayExtension>? Extensions { get; set; }

    /// <inheritdoc/>
    public void SerializeAsV1(IOpenApiWriter writer) => SerializeInternal(writer, OverlaySpecVersion.Overlay1_0);

    /// <inheritdoc/>
    public void SerializeAsV1_1(IOpenApiWriter writer) => SerializeInternal(writer, OverlaySpecVersion.Overlay1_1);

    private void SerializeInternal(IOpenApiWriter writer, OverlaySpecVersion version)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(Name);

        writer.WriteStartObject();
        writer.WriteRequiredProperty(OverlayConstants.ReusableActionParameterNameFieldName, Name);

        if (Default != null)
        {
            writer.WriteProperty(OverlayConstants.ReusableActionParameterDefaultFieldName, Default);
        }

        writer.WriteOverlayExtensions(Extensions, version);
        writer.WriteEndObject();
    }
}