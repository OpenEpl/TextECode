using OpenEpl.TextECode.Internal.ProgramElems;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserFormElement : BaseVariableElem, IMemberElem
    {
        public string Name { get; }

        public UserFormElement(TextECodeRestorer p, string name, int formId, int id, BaseDataTypeElem dataType) : base(p, id)
        {
            Name = name;
            ClassId = formId;
            DataType = dataType;
        }

        public override BaseDataTypeElem DataType { get; }

        public int ClassId { get; }

        public int MemberId => Id;

        public int? ImplicitTarget => null;
    }
}
