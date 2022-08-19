using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Antlr4.Runtime;

namespace OpenEpl.TextECode.Internal
{
    internal class TokenUtils
    {
        public static string ReadStringItem(IToken node)
        {
            if (node == null)
            {
                return null;
            }
            var x = node.Text;
            if (x.Length >= 2 && x[0] == '"' && x[^1] == '"')
            {
                x = x[1..^1];
            }
            return x;
        }
        public static int[] ReadArrayUBound(IToken node)
        {
            if (node == null)
            {
                return Array.Empty<int>();
            }
            var x = node.Text;
            if (x.Length >= 2 && x[0] == '"' && x[^1] == '"')
            {
                x = x[1..^1];
            }
            return x.Split(',').Select(x =>
            {
                int.TryParse(x.Trim(), out var result);
                return result;
            }).ToArray();
        }
        public static DateTime ReadDateTimeLiteral(IToken token)
        {
            return ReadDateTimeLiteral(token.Text);
        }
        public static DateTime ReadDateTimeLiteral(string text)
        {
            return DateTime.ParseExact(text, new string[] { "[yyyy年MM月dd日HH时mm分ss秒]", "[yyyy年MM月dd日]" }, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
        }
        public static string GetRawText(ParserRuleContext context)
        {
            if (context == null)
            {
                return null;
            }
            return context.Start.InputStream.GetText(new Antlr4.Runtime.Misc.Interval(
                            context.Start.StartIndex,
                            context.Stop.StopIndex));
        }
    }
}
