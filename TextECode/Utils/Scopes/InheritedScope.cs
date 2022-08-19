using OpenEpl.TextECode.Utils.Scopes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Utils.Scopes
{
    public class InheritedScope<TKey, TValue> : IScope<TKey, TValue>
    {
        private IScope<TKey, TValue> Child { get; }
        private IScope<TKey, TValue> Parent { get; }

        public InheritedScope(IScope<TKey, TValue> child, IScope<TKey, TValue> parent)
        {
            Child = child ?? throw new ArgumentNullException(nameof(child));
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public TValue this[TKey key]
        {
            get
            {
                var existence = Child.TryGetValue(key, out var value);
                return existence switch
                {
                    KeyExistence.Available => value,
                    KeyExistence.Ambiguous => throw new AmbiguousKeyException($"{key} is ambiguous"),
                    KeyExistence.NotFound => Parent[key],
                    _ => throw new Exception($"Unknown existence: {existence}"),
                };
            }
        }

        public KeyExistence GetExistence(TKey key)
        {
            var existence = Child.GetExistence(key);
            if (existence != KeyExistence.NotFound)
            {
                return existence;
            }
            return Parent.GetExistence(key);
        }

        public KeyExistence TryGetValue(TKey key, out TValue value)
        {
            var existence = Child.TryGetValue(key, out value);
            if (existence != KeyExistence.NotFound)
            {
                return existence;
            }
            return Parent.TryGetValue(key, out value);
        }

        public override string ToString()
        {
            return $"{nameof(InheritedScope<TKey, TValue>)}({Child}, {Parent})";
        }
    }
}
