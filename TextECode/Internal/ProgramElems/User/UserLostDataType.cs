using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserLostDataType : BaseDataTypeElem
    {
        public string Name { get; }

        public UserLostDataType(TextECodeRestorer p, string name, int type) : base(p, p.AllocId(type))
        {
            Name = name;
        }
        public UserLostDataType(TextECodeRestorer p, string name) : this(p, name, EplSystemId.Type_Struct)
        {
        }
        public void Finish()
        {
            P.Losable.RemovedDefinedItems.Add(new RemovedDefinedItemInfo(Id)
            {
                Name = Name
            });
        }
    }
}
