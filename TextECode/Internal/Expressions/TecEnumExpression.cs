using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Internal.ProgramElems.External;
using QIQI.EProjectFile;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal class TecEnumExpression : TecExpression
    {
        private readonly IMemberElem MemberElem;

        public TecEnumExpression(CodeTranslatorContext c, IMemberElem memberElem) : base(c)
        {
            MemberElem = memberElem;
        }

        public override BaseDataTypeElem ResultDataType => MemberElem.DataType ?? P.SystemDataTypes.Int;

        public override Expression ToNative()
        {
            EplSystemId.DecomposeLibDataTypeId(MemberElem.ClassId, out var lib, out var type);
            return new EmnuConstantExpression(type, lib, MemberElem.MemberId);
        }
    }
}
