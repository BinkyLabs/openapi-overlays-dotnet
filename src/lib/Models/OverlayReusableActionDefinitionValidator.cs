namespace BinkyLabs.OpenApi.Overlays;

#pragma warning disable BOO002
internal static class OverlayReusableActionDefinitionValidator
{
    internal static Dictionary<string, OverlayReusableActionParameter> BuildDefinitionsByName(
        IList<OverlayReusableActionParameter> definitions,
        string definitionKind)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentException.ThrowIfNullOrEmpty(definitionKind);

        var definitionsByName = new Dictionary<string, OverlayReusableActionParameter>(definitions.Count, StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            if (definition is null)
            {
                throw new InvalidOperationException($"Reusable action {definitionKind} definition cannot be null.");
            }

            if (string.IsNullOrEmpty(definition.Name) || !IsValidReusableDefinitionName(definition.Name))
            {
                throw new InvalidOperationException(
                    $"Reusable action {definitionKind} definition name '{definition.Name ?? "<null>"}' is invalid. " +
                    "Names must match: ( ALPHA / '_' ) *( ALPHA / DIGIT / '_' ).");
            }

            if (!definitionsByName.TryAdd(definition.Name, definition))
            {
                throw new InvalidOperationException(
                    $"Duplicate reusable action {definitionKind} definition name '{definition.Name}'.");
            }
        }

        return definitionsByName;
    }

    private static bool IsValidReusableDefinitionName(string name)
    {
        if (name.Length == 0 || (!IsAsciiAlpha(name[0]) && !IsUnderscore(name[0])))
        {
            return false;
        }

        for (int index = 1; index < name.Length; index++)
        {
            var current = name[index];
            if (!IsAsciiAlpha(current) && !IsAsciiDigit(current) && !IsUnderscore(current))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAsciiAlpha(char value) =>
        (value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z');

    private static bool IsAsciiDigit(char value) => value >= '0' && value <= '9';

    private static bool IsUnderscore(char value) => value == '_';
}
#pragma warning restore BOO002