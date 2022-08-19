using OpenEpl.TextECode.Utils.Scopes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Utils.Scopes
{
    public class ParallelScope<TKey, TValue> : IScope<TKey, TValue>
    {
        private readonly IEnumerable<IScope<TKey, TValue>> scopes;

        public ParallelScope(IEnumerable<IScope<TKey, TValue>> scopes)
        {
            this.scopes = scopes;
        }

        public TValue this[TKey key]
        {
            get
            {
                var enumerator = scopes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var existence = enumerator.Current.TryGetValue(key, out var value);
                    switch (existence)
                    {
                        case KeyExistence.Available:
                            while (enumerator.MoveNext())
                            {
                                if (enumerator.Current.GetExistence(key) != KeyExistence.NotFound)
                                {
                                    throw new AmbiguousKeyException($"{key} is ambiguous");
                                }
                            }
                            return value;
                        case KeyExistence.Ambiguous:
                            throw new AmbiguousKeyException($"{key} is ambiguous");
                        case KeyExistence.NotFound:
                            break;
                        default:
                            throw new Exception($"Unknown existence: {existence}");
                    }
                }
                throw new KeyNotFoundException($"{key} is not found");
            }
        }

        public KeyExistence GetExistence(TKey key)
        {
            var enumerator = scopes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var existence = enumerator.Current.GetExistence(key);
                switch (existence)
                {
                    case KeyExistence.Available:
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.GetExistence(key) != KeyExistence.NotFound)
                            {
                                return KeyExistence.Ambiguous;
                            }
                        }
                        return KeyExistence.Available;
                    case KeyExistence.Ambiguous:
                        return KeyExistence.Ambiguous;
                    case KeyExistence.NotFound:
                        break;
                    default:
                        throw new Exception($"Unknown existence: {existence}");
                }
            }
            return KeyExistence.NotFound;
        }

        public KeyExistence TryGetValue(TKey key, out TValue value)
        {
            var enumerator = scopes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var existence = enumerator.Current.TryGetValue(key, out value);
                switch (existence)
                {
                    case KeyExistence.Available:
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.GetExistence(key) != KeyExistence.NotFound)
                            {
                                // ambiguous key
                                value = default;
                                return KeyExistence.Ambiguous;
                            }
                        }
                        return KeyExistence.Available;
                    case KeyExistence.Ambiguous:
                        return KeyExistence.Ambiguous;
                    case KeyExistence.NotFound:
                        break;
                    default:
                        throw new Exception($"Unknown existence: {existence}");
                }
            }
            value = default;
            return KeyExistence.NotFound;
        }

        public override string ToString()
        {
            return $"{nameof(ParallelScope<TKey, TValue>)}({string.Join(", ", scopes)})";
        }
    }
}
