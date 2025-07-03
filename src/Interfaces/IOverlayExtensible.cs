// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Represents an Extensible Open API element.
/// </summary>
public interface IOverlayExtensible : IOpenApiElement
{
    /// <summary>
    /// Specification extensions.
    /// </summary>
    IDictionary<string, IOverlayExtension>? Extensions { get; set; }
}