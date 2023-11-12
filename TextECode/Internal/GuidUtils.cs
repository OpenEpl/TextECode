using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenEpl.TextECode.Internal
{
    internal class GuidUtils
    {
        public static Guid ParseGuidLosely(string x)
        {
            return Guid.ParseExact(x.Replace("-", "").Replace("{", "").Replace("}", ""), "N");
        }
    }
}
