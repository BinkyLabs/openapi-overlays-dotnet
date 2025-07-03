// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

using BinkyLabs.OpenApi.Overlays.Reader;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Defines an exception indicating OpenAPI Reader encountered an issue while reading.
/// </summary>
[Serializable]
public class OverlayReaderException : OverlayException
{
    /// <summary>
    /// Initializes the <see cref="OverlayReaderException"/> class.
    /// </summary>
    public OverlayReaderException() { }

    /// <summary>
    /// Initializes the <see cref="OverlayReaderException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Plain text error message for this exception.</param>
    public OverlayReaderException(string message) : base(message) { }

    /// <summary>
    /// Initializes the <see cref="OverlayReaderException"/> class with a custom message.
    /// </summary>
    /// <param name="message">Plain text error message for this exception.</param>
    /// <param name="context">Context of current parsing process.</param>
    public OverlayReaderException(string message, ParsingContext context) : base(message)
    {
        Pointer = context.GetLocation();
    }

    /// <summary>
    /// Initializes the <see cref="OverlayReaderException"/> class with a custom message and inner exception.
    /// </summary>
    /// <param name="message">Plain text error message for this exception.</param>
    /// <param name="innerException">Inner exception that caused this exception to be thrown.</param>
    public OverlayReaderException(string message, Exception innerException) : base(message, innerException) { }
}
