
// Licensed under the MIT license.

namespace BinkyLabs.OpenApi.Overlays.Reader
{
    internal class FixedFieldMap<T> : Dictionary<string, Action<T, ParseNode>>
    {
        public FixedFieldMap() : base()
        {

        }
        public FixedFieldMap(FixedFieldMap<T> source) : base(source)
        {

        }
        public FixedFieldMap(FixedFieldMap<T> source, HashSet<string> except) : base(source.Where(kv => !except.Contains(kv.Key)))
        {

        }
    }
}