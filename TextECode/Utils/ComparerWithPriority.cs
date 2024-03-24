using System;
using System.Collections.Generic;

namespace OpenEpl.TextECode.Utils
{
    public class ComparerWithPriority<T> : IComparer<T>
    {
        private readonly Func<T, int> _priorityFunc;
        private readonly Comparison<T> _secondaryComparer;

        public ComparerWithPriority(Func<T, int> priorityFunc, Comparison<T> secondaryComparer)
        {
            _priorityFunc = priorityFunc ?? throw new ArgumentNullException(nameof(priorityFunc));
            _secondaryComparer = secondaryComparer ?? throw new ArgumentNullException(nameof(secondaryComparer));
        }

        public int Compare(T x, T y)
        {
            var xPriority = _priorityFunc(x);
            var yPriority = _priorityFunc(y);
            if (xPriority != yPriority)
            {
                return xPriority - yPriority;
            }
            else
            {
                return _secondaryComparer(x, y);
            }
        }
    }
}
