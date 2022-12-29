using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal
{
    internal static class StringUtils
    {
        public static string NullIfEmpty(this string x) => string.Empty.Equals(x) ? null : x;
    }
}
