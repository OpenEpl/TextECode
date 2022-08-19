using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecBoolLiterall : TecExpression
    {
        private readonly bool Value;
        public override BaseDataTypeElem ResultDataType => P.SystemDataTypes.Bool;

        public TecBoolLiterall(CodeTranslatorContext c, bool value) : base(c)
        {
            Value = value;
        }

        public override Expression ToNative()
        {
            return BoolLiteral.ValueOf(Value);
        }
    }
}
