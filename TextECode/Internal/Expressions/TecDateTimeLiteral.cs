using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecDateTimeLiteral : TecExpression
    {
        private readonly DateTime Value;
        public override BaseDataTypeElem ResultDataType => P.GetTypeElem("日期时间型");

        public TecDateTimeLiteral(CodeTranslatorContext c, DateTime value) : base(c)
        {
            Value = value;
        }

        public override Expression ToNative()
        {
            return new DateTimeLiteral(Value);
        }
    }
}
