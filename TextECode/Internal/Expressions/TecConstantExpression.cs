using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Internal.ProgramElems.External;
using OpenEpl.TextECode.Internal.ProgramElems.User;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecConstantExpression : TecExpression
    {
        private readonly ProgramElem Elem;
        public override BaseDataTypeElem ResultDataType => Elem switch
        {
            ExternalConstantElem elem => elem.DataType,
            UserConstantElem elem => elem.DataType,
            _ => throw new Exception("Unsupported constant type")
        };

        public TecConstantExpression(CodeTranslatorContext c, ProgramElem elem) : base(c)
        {
            Elem = elem ?? throw new ArgumentNullException(nameof(elem));
        }

        public override Expression ToNative()
        {
            return Elem switch
            {
                ExternalConstantElem elem => new ConstantExpression(checked((short)elem.LibId), elem.ConstantId),
                UserConstantElem elem => new ConstantExpression(elem.Id),
                _ => throw new Exception("Unsupported constant type")
            };
        }
    }
}
