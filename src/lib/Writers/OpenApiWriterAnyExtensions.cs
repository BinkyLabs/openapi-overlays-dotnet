
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Writers;

/// <summary>
/// Extensions methods for writing the <see cref="JsonNodeExtension"/>
/// </summary>
public static class OpenApiWriterAnyExtensions
{
    /// <summary>
    /// Write the specification extensions
    /// </summary>
    /// <param name="writer">The Open API writer.</param>
    /// <param name="extensions">The specification extensions.</param>
    /// <param name="specVersion">Version of the OpenAPI specification that that will be output.</param>
    public static void WriteOverlayExtensions(this IOpenApiWriter writer, IDictionary<string, IOverlayExtension>? extensions, OverlaySpecVersion specVersion)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (extensions != null)
        {
            foreach (var item in extensions)
            {
                writer.WritePropertyName(item.Key);

                if (item.Value == null)
                {
                    writer.WriteNull();
                }
                else
                {
                    item.Value.Write(writer, specVersion);
                }
            }
        }
    }
}