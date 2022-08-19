using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecNumberLiteral : TecExpression
    {
        private readonly double Value;
        public override BaseDataTypeElem ResultDataType => P.SystemDataTypes.Double;

        public TecNumberLiteral(CodeTranslatorContext c, double value) : base(c)
        {
            Value = value;
        }

        public override Expression ToNative()
        {
            return new NumberLiteral(Value);
        }
    }
}
