using System.Diagnostics.CodeAnalysis;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Specifies the source of a parameter value.
/// This enum is experimental and not part of the OpenAPI Overlay specification v1.0.0.
/// This enum is an implementation of <see href="https://github.com/OAI/Overlay-Specification/pull/238">the action parameters proposal</see>.
/// </summary>
[Experimental("BOO002", UrlFormat = "https://github.com/OAI/Overlay-Specification/pull/238")]
public enum ParameterValueSource
{
    /// <summary>
    /// Values are provided inline in the overlay document.
    /// </summary>
    Inline,

    /// <summary>
    /// Values are sourced from environment variables.
    /// </summary>
    Environment
}