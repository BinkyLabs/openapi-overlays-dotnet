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

    /// <summary>
    /// Combines this components object with other components objects.
    /// Actions with the same key are overwritten by later components objects.
    /// </summary>
    /// <param name="others">The other components objects to merge.</param>
    /// <returns>A new components object containing merged actions.</returns>
    public OverlayComponents CombineWith(params OverlayComponents[] others)
    {
        return Combine([this, .. others]);
    }

    /// <summary>
    /// Combines multiple components objects into a single components object.
    /// </summary>
    /// <param name="componentsArray">The components objects to combine.</param>
    /// <returns>A new components object containing merged actions.</returns>
    /// <exception cref="ArgumentException">Thrown when no components objects are provided.</exception>
    public static OverlayComponents Combine(params OverlayComponents[] componentsArray)
    {
        if (componentsArray is not { Length: > 0 })
        {
            throw new ArgumentException("At least one components object must be provided for combination.", nameof(componentsArray));
        }
        var actions = componentsArray
            .Select(static x => x.Actions)
            .OfType<IDictionary<string, OverlayReusableAction>>()
            .SelectMany(static x => x)
            .GroupBy(static x => x.Key, StringComparer.Ordinal)
            .ToDictionary(static x => x.Key,
                        static x => x.Last().Value,
                        StringComparer.Ordinal);

        return new OverlayComponents
        {
            Actions = actions
        };
    }

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