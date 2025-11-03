
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

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
    }
}