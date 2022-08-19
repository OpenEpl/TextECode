using OpenEpl.TextECode.Internal.ProgramElems;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.External
{
    internal class ExternalMemberElem: ProgramElem, IMemberElem
    {
        public int ClassId { get; }
        public int MemberId { get; }
        public BaseDataTypeElem DataType { get; }
        public string Name { get; }
        public int? ImplicitTarget { get; }

        public ExternalMemberElem(TextECodeRestorer p, int classId, int memberId, string name, BaseDataTypeElem dataType, int? implicitTarget = null) : base(p)
        {
            ClassId = classId;
            MemberId = memberId;
            Name = name;
            DataType = dataType;
            ImplicitTarget = implicitTarget;
        }
    }
}
