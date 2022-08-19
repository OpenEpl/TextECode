using OpenEpl.ELibInfo;
using OpenEpl.TextECode.Internal.ProgramElems;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.External
{
    internal class ExternalMethodElem : BaseCallableElem
    {
        public int LibId { get; }
        public int MethodId { get; }
        public override BaseDataTypeElem ReturnDataType { get; }
        public string Name { get; }
        public ExternalMethodElem(TextECodeRestorer p, int libId, int methodId, string name, BaseDataTypeElem returnDataType) : base(p)
        {
            LibId = libId;
            MethodId = methodId;
            Name = name;
            ReturnDataType = returnDataType;
        }
    }
}
