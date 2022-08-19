using OpenEpl.TextECode.Internal.ProgramElems;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.System
{
    internal class SystemDataTypeElem : BaseDataTypeElem
    {
        public string Name { get; }

        public SystemDataTypeElem(TextECodeRestorer p, int id, string name) : base(p, id)
        {
            Name = name;
        }
    }
}
