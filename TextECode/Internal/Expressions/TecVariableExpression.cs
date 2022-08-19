using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecVariableExpression : TecExpression
    {
        private readonly BaseVariableElem Elem;
        public override BaseDataTypeElem ResultDataType => Elem.DataType ?? P.SystemDataTypes.Int;

        public TecVariableExpression(CodeTranslatorContext c, BaseVariableElem elem) : base(c)
        {
            Elem = elem ?? throw new ArgumentNullException(nameof(elem));
        }

        public override Expression ToNative()
        {
            return new VariableExpression(Elem.Id);
        }
    }
}
