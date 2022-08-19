using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Internal.ProgramElems.External;
using OpenEpl.TextECode.Internal.ProgramElems.User;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecAddrOfExpression : TecExpression
    {
        private readonly BaseCallableElem Elem;
        public TecAddrOfExpression(CodeTranslatorContext c, BaseCallableElem elem) : base(c)
        {
            Elem = elem;
        }

        public override BaseDataTypeElem ResultDataType => P.GetTypeElem("子程序指针");

        public override Expression ToNative()
        {
            short libId;
            int methodId;
            switch (Elem)
            {
                case ExternalMethodElem externalMethodElem:
                    libId = checked((short)externalMethodElem.LibId);
                    methodId = externalMethodElem.MethodId;
                    break;
                case UserMethodElem userMethodElem:
                    libId = -2;
                    methodId = userMethodElem.Id;
                    break;
                default:
                    throw new Exception();
            }
            if (libId != -2)
            {
                throw new Exception("Only user-defined or external user-defined methods are suitable for AddrOf expression");
            }
            return new MethodPtrExpression(methodId);
        }
    }
}
