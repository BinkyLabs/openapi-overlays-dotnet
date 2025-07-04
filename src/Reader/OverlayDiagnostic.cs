
// Licensed under the MIT license.

using System.Collections.Generic;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays.Reader;

/// <summary>
/// Object containing all diagnostic information related to Open API parsing.
/// </summary>
public class OverlayDiagnostic : IDiagnostic
{
    /// <summary>
    /// List of all errors.
    /// </summary>
    public IList<OpenApiError> Errors { get; set; } = [];

    /// <summary>
    /// List of all warnings
    /// </summary>
    public IList<OpenApiError> Warnings { get; set; } = [];

    /// <summary>
    /// Overlay specification version of the document parsed.
    /// </summary>
    public OverlaySpecVersion SpecificationVersion { get; set; }
}