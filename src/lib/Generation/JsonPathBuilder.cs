namespace BinkyLabs.OpenApi.Overlays.Generation;

/// <summary>
/// Helper class for building JSONPath expressions.
/// </summary>
internal static class JsonPathBuilder
{
    /// <summary>
    /// Builds a JSONPath by appending a property name to an existing path.
    /// </summary>
    /// <param name="basePath">The base JSONPath.</param>
    /// <param name="propertyName">The property name to append.</param>
    /// <returns>The combined JSONPath.</returns>
    public static string BuildPath(string basePath, string propertyName)
    {
        var escaped = EscapePropertyName(propertyName);
        
        if (string.IsNullOrEmpty(basePath) || basePath == "$")
        {
            if (escaped.StartsWith("[", StringComparison.Ordinal))
            {
                return $"${escaped}";
            }
            return $"$.{escaped}";
        }

        if (escaped.StartsWith("[", StringComparison.Ordinal))
        {
            return $"{basePath}{escaped}";
        }

        return $"{basePath}.{escaped}";
    }

    /// <summary>
    /// Builds a JSONPath by appending an array index to an existing path.
    /// </summary>
    /// <param name="basePath">The base JSONPath.</param>
    /// <param name="index">The array index to append.</param>
    /// <returns>The combined JSONPath.</returns>
    public static string BuildPath(string basePath, int index)
    {
        if (string.IsNullOrEmpty(basePath) || basePath == "$")
        {
            return $"$[{index}]";
        }

        return $"{basePath}[{index}]";
    }

    private static string EscapePropertyName(string propertyName)
    {
        // Check if the property name needs to be wrapped in brackets
        // This is necessary for property names with special characters or that don't follow identifier rules
        if (NeedsEscaping(propertyName))
        {
            // Escape single quotes in the property name
            var escaped = propertyName.Replace("'", "\\'");
            return $"['{escaped}']";
        }

        return propertyName;
    }

    private static bool NeedsEscaping(string propertyName)
    {
        // Check if the property name contains special characters that require bracketed notation
        if (string.IsNullOrEmpty(propertyName))
        {
            return true;
        }

        // Property names with spaces, slashes, or other special characters need bracketing
        return propertyName.Contains(' ') ||
               propertyName.Contains('/') ||
               propertyName.Contains('-') ||
               propertyName.Contains('.') ||
               propertyName.Contains('[') ||
               propertyName.Contains(']') ||
               propertyName.Contains('\'') ||
               propertyName.Contains('"') ||
               propertyName.Contains('~') ||
               !char.IsLetter(propertyName[0]);
    }
}
