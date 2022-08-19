using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Utils.Scopes
{
    public class EmptyScope<TKey, TValue> : IScope<TKey, TValue>
    {
        public static EmptyScope<TKey, TValue> Instance { get; } = new();

        private EmptyScope()
        {
        }

        public TValue this[TKey key]
        {
            get
            {
                throw new KeyNotFoundException($"{key} is not found");
            }
        }

        public KeyExistence GetExistence(TKey key)
        {
            return KeyExistence.NotFound;
        }

        public KeyExistence TryGetValue(TKey key, out TValue value)
        {
            value = default;
            return KeyExistence.NotFound;
        }
    }
}
