using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Utils.Scopes
{
    public interface IMutableScope<TKey, TValue>: IScope<TKey, TValue>
    {
        public void Add(TKey key, TValue value);
    }
}
