using OpenEpl.TextECode.Utils.Scopes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Utils.Scopes
{
    public interface IScope<TKey, TValue>
    {
        public TValue this[TKey key] { get; }
        public KeyExistence GetExistence(TKey key);
        public KeyExistence TryGetValue(TKey key, out TValue value);
    }
}
