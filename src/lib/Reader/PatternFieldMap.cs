// Licensed under the MIT license.

using System.Text.Json.Nodes;

namespace BinkyLabs.OpenApi.Overlays.Reader
{
    internal class PatternFieldMap<T> : Dictionary<Func<string, bool>, Action<T, string, JsonNode, ParsingContext>>
    {
    }
}