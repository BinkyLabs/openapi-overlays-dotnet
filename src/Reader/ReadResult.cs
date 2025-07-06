using BinkyLabs.OpenApi.Overlays.Reader;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents the result of reading and parsing an OpenAPI overlay document.
/// </summary>
public class ReadResult
{
    /// <summary>
    /// The parsed OverlayDocument. Null will be returned if the document could not be parsed.
    /// </summary>
    public OverlayDocument? Document { get; set; }

    /// <summary>
    /// OverlayDiagnostic contains the errors reported while parsing.
    /// </summary>
    public OverlayDiagnostic? Diagnostic { get; set; }

    /// <summary>
    /// Deconstructs the result for easier assignment on the client application.
    /// </summary>
    /// <param name="document">The parsed overlay document.</param>
    /// <param name="diagnostic">The diagnostic information containing parsing errors.</param>
    public void Deconstruct(out OverlayDocument? document, out OverlayDiagnostic? diagnostic)
    {
        document = Document;
        diagnostic = Diagnostic;
    }
}