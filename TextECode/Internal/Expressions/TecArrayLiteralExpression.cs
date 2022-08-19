using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecArrayLiteralExpression : TecExpression
    {
        private readonly List<TecExpression> Expressions;

        public TecArrayLiteralExpression(CodeTranslatorContext c, List<TecExpression> expressions) : base(c)
        {
            Expressions = expressions;
        }

        public override BaseDataTypeElem ResultDataType => Expressions?.FirstOrDefault()?.ResultDataType ?? P.SystemDataTypes.Int;

        public override Expression ToNative()
        {
            var expr = new ArrayLiteralExpression();
            if (Expressions != null)
            {
                foreach (var item in Expressions)
                {
                    expr.Add(item.ToNative());
                }
            }
            return expr;
        }
    }
}
