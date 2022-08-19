using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Internal.ProgramElems.External;
using QIQI.EProjectFile;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecAccessMemberExpression : TecExpression
    {
        private readonly TecExpression Target;
        private readonly IMemberElem MemberElem;

        public TecAccessMemberExpression(CodeTranslatorContext c, TecExpression target, IMemberElem memberElem) : base(c)
        {
            Target = target;
            MemberElem = memberElem;
        }

        public override BaseDataTypeElem ResultDataType => MemberElem.DataType ?? P.SystemDataTypes.Int;

        public override Expression ToNative()
        {
            var dataTypeId = MemberElem.ClassId;
            var targetNative = Target?.ToNative() ?? new VariableExpression((int)MemberElem.ImplicitTarget);
            if (EplSystemId.IsLibDataType(dataTypeId))
            {
                EplSystemId.DecomposeLibDataTypeId(dataTypeId, out var lib, out var type);
                return new AccessMemberExpression(targetNative, lib, type, MemberElem.MemberId);
            }
            else
            {
                return new AccessMemberExpression(targetNative, dataTypeId, MemberElem.MemberId);
            }
        }
    }
}
