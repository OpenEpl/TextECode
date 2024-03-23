using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Utils.Scopes
{
    [Serializable]
    public class AmbiguousKeyException : Exception
    {
        public AmbiguousKeyException() { }
        public AmbiguousKeyException(string message) : base(message) { }
        public AmbiguousKeyException(string message, Exception inner) : base(message, inner) { }
    }
}
