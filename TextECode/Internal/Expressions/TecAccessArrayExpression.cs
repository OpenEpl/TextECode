using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecAccessArrayExpression: TecExpression
    {
        private readonly TecExpression Target;
        private readonly TecExpression Index;

        public TecAccessArrayExpression(CodeTranslatorContext c, TecExpression target, TecExpression index) : base(c)
        {
            Target = target;
            Index = index;
        }

        public override BaseDataTypeElem ResultDataType => Target.ResultDataType switch
        {
            var x when x.Id == EplSystemId.DataType_Bin => P.SystemDataTypes.Byte,
            var x => x
        };

        public override Expression ToNative()
        {
            return new AccessArrayExpression(Target.ToNative(), Index.ToNative());
        }
    }
}
