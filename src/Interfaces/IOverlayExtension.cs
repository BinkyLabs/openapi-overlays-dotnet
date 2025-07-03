// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Interface required for implementing any custom extension
/// </summary>
public interface IOverlayExtension
{
    /// <summary>
    /// Write out contents of custom extension
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="specVersion">Version of the overlay specification that that will be output.</param>
    void Write(IOpenApiWriter writer, OverlaySpecVersion specVersion);
}
