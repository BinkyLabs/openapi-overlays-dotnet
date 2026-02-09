// Licensed under the MIT license.

using BinkyLabs.OpenApi.Overlays.Reader;

namespace BinkyLabs.OpenApi.Overlays.Generation;

/// <summary>
/// Represents the result of generating an overlay document.
/// </summary>
public class OverlayGenerationResult
{
    /// <summary>
    /// The generated OverlayDocument. Null will be returned if generation failed.
    /// </summary>
    public OverlayDocument? Document { get; set; }

    /// <summary>
    /// OverlayDiagnostic contains any errors or warnings reported during generation.
    /// </summary>
    public OverlayDiagnostic Diagnostic { get; set; } = new();

    /// <summary>
    /// Deconstructs the result for easier assignment.
    /// </summary>
    /// <param name="document">The generated overlay document.</param>
    /// <param name="diagnostic">The diagnostic information containing errors and warnings.</param>
    public void Deconstruct(out OverlayDocument? document, out OverlayDiagnostic diagnostic)
    {
        document = Document;
        diagnostic = Diagnostic;
    }

    /// <summary>
    /// Deconstructs the result for easier assignment.
    /// </summary>
    /// <param name="document">The generated overlay document.</param>
    public void Deconstruct(out OverlayDocument? document)
    {
        Deconstruct(out document, out _);
    }
}
