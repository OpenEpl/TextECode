using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Internal.ProgramElems.External;
using OpenEpl.TextECode.Internal.ProgramElems.User;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecCallExpression : TecExpression
    {
        private readonly BaseCallableElem Elem;
        private readonly TecExpression Target;
        private readonly TecArgListExpression Args;
        private readonly bool InvokeSpecial;

        public TecCallExpression(CodeTranslatorContext c, BaseCallableElem elem, TecExpression target, TecArgListExpression args, bool invokeSpecial = false) : base(c)
        {
            Elem = elem;
            Target = target;
            Args = args;
            InvokeSpecial = invokeSpecial;
        }

        public override BaseDataTypeElem ResultDataType => Elem.ReturnDataType;

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
                case UserDllDeclareElem userDllDeclareElem:
                    libId = -3;
                    methodId = userDllDeclareElem.Id;
                    break;
                default:
                    throw new Exception();
            }
            return new CallExpression(libId, methodId, (ParamListExpression)Args.ToNative())
            {
                Target = Target?.ToNative(),
                InvokeSpecial = InvokeSpecial
            };
        }
    }
}
