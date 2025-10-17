using BinkyLabs.OpenApi.Overlays.Reader;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Result of applying overlays to an OpenAPI document
/// </summary>
public class OverlayApplicationResult
{
    /// <summary>
	/// The resulting OpenAPI document after applying overlays, or null if application failed
	/// </summary>
    public OpenApiDocument? Document { get; init; }
    /// <summary>
	/// Diagnostics from applying the overlays
	/// </summary>
    public required OverlayDiagnostic Diagnostic { get; init; }
    /// <summary>
	/// Diagnostics from reading the updated OpenAPI document
	/// </summary>
    public OpenApiDiagnostic? OpenApiDiagnostic { get; init; }

    /// <summary>
    /// Indicates whether the overlay application was successful
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
	/// Deconstructs the OverlayApplicationResult into its components
	/// </summary>
	/// <param name="document">The resulting OpenAPI document after applying overlays, or null if application failed</param>
	/// <param name="diagnostic">Diagnostics from applying the overlays</param>
	/// <param name="openApiDiagnostic">Diagnostics from reading the updated OpenAPI document</param>
	/// <param name="isSuccessful">Indicates whether the overlay application was successful</param>
    public void Deconstruct(out OpenApiDocument? document, out OverlayDiagnostic diagnostic, out OpenApiDiagnostic? openApiDiagnostic, out bool isSuccessful)
    {
        document = Document;
        diagnostic = Diagnostic;
        openApiDiagnostic = OpenApiDiagnostic;
        isSuccessful = IsSuccessful;
    }
    /// <summary>
	/// Deconstructs the OverlayApplicationResult into its components
	/// </summary>
	/// <param name="document">The resulting OpenAPI document after applying overlays, or null if application failed</param>
	/// <param name="diagnostic">Diagnostics from applying the overlays</param>
	/// <param name="openApiDiagnostic">Diagnostics from reading the updated OpenAPI document</param>
    public void Deconstruct(out OpenApiDocument? document, out OverlayDiagnostic diagnostic, out OpenApiDiagnostic? openApiDiagnostic)
    {
        Deconstruct(out document, out diagnostic, out openApiDiagnostic, out _);
    }
    /// <summary>
	/// Deconstructs the OverlayApplicationResult into its components
	/// </summary>
	/// <param name="document">The resulting OpenAPI document after applying overlays, or null if application failed</param>
	/// <param name="diagnostic">Diagnostics from applying the overlays</param>
    public void Deconstruct(out OpenApiDocument? document, out OverlayDiagnostic diagnostic)
    {
        Deconstruct(out document, out diagnostic, out _);
    }
    /// <summary>
	/// Deconstructs the OverlayApplicationResult into its components
	/// </summary>
	/// <param name="document">The resulting OpenAPI document after applying overlays, or null if application failed</param>
    public void Deconstruct(out OpenApiDocument? document)
    {
        Deconstruct(out document, out _);
    }
}