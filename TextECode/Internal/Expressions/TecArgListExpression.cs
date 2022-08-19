using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecArgListExpression : TecExpression
    { 
        private readonly TecExpression[] Args;
        public override BaseDataTypeElem ResultDataType => throw new NotSupportedException();

        public TecArgListExpression(CodeTranslatorContext c, TecExpression[] args) : base(c)
        {
            Args = args;
        }

        public override Expression ToNative()
        {
            var native = new ParamListExpression();
            foreach (var item in Args)
            {
                native.Add(item?.ToNative());
            }
            return native;
        }

        public TecExpression GetOrDefault(int index)
        {
            if (index < 0 ||index >= Args.Length)
            {
                return default;
            }
            return Args[index];
        }
    }
}
