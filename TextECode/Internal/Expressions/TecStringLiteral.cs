using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecStringLiteral : TecExpression
    {
        private readonly string Value;
        public override BaseDataTypeElem ResultDataType => P.GetTypeElem("文本型");

        public TecStringLiteral(CodeTranslatorContext c, string value) : base(c)
        {
            Value = value;
        }

        public override Expression ToNative()
        {
            return new StringLiteral(Value);
        }
    }
}
