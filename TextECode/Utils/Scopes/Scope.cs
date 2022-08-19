using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Utils.Scopes
{
    public class Scope<TKey, TValue> : IMutableScope<TKey, TValue>
    {
        private readonly Dictionary<TKey, List<TValue>> data = new();

        public TValue this[TKey key]
        {
            get
            {
                if (data.TryGetValue(key, out var valueList))
                {
                    switch(valueList.Count)
                    {
                        case 0:
                            break;
                        case 1:
                            return valueList[0];
                        default:
                            throw new AmbiguousKeyException($"{key} is ambiguous");
                    }
                }
                throw new KeyNotFoundException($"{key} is not found");
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (data.TryGetValue(key, out var valueList))
            {
                valueList.Add(value);
            }
            else
            {
                data.Add(key, new List<TValue>(1) { value });
            }
        }

        public KeyExistence GetExistence(TKey key)
        {
            if (data.TryGetValue(key, out var valueList))
            {
                switch (valueList.Count)
                {
                    case 0:
                        break;
                    case 1:
                        return KeyExistence.Available;
                    default:
                        return KeyExistence.Ambiguous;
                }
            }
            return KeyExistence.NotFound;
        }

        public KeyExistence TryGetValue(TKey key, out TValue value)
        {
            if (data.TryGetValue(key, out var valueList))
            {
                switch (valueList.Count)
                {
                    case 0:
                        break;
                    case 1:
                        value = valueList[0];
                        return KeyExistence.Available;
                    default:
                        value = default;
                        return KeyExistence.Ambiguous;
                }
            }
            value = default;
            return KeyExistence.NotFound;
        }
    }
}
