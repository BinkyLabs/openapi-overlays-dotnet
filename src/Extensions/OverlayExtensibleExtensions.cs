// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

using Microsoft.OpenApi;

namespace BinkyLabs.OpenApi.Overlays;

/// <summary>
/// Extension methods to verify validity and add an extension to Extensions property.
/// </summary>
public static class OverlayExtensibleExtensions
{
    /// <summary>
    /// Add extension into the Extensions
    /// </summary>
    /// <typeparam name="T"><see cref="IOpenApiExtensible"/>.</typeparam>
    /// <param name="element">The extensible Open API element. </param>
    /// <param name="name">The extension name.</param>
    /// <param name="any">The extension value.</param>
    public static void AddExtension<T>(this T element, string name, IOverlayExtension any)
        where T : IOverlayExtensible
    {
        ArgumentNullException.ThrowIfNull(element);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(any);

        if (!name.StartsWith(OpenApiConstants.ExtensionFieldNamePrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new OpenApiException(string.Format("The extension name must start with x-, current name {0}", name));
        }

        element.Extensions ??= new Dictionary<string, IOverlayExtension>();
        element.Extensions[name] = any;
    }
}