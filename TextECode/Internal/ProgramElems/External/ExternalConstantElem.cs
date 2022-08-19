using OpenEpl.TextECode.Internal.ProgramElems;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.External
{
    internal class ExternalConstantElem : ProgramElem
    {
        public int LibId { get; }
        public int ConstantId { get; }
        public BaseDataTypeElem DataType { get; }
        public string Name { get; }
        public ExternalConstantElem(TextECodeRestorer p, int libId, int constantId, string name, BaseDataTypeElem dataType) : base(p)
        {
            LibId = libId;
            ConstantId = constantId;
            Name = name;
            DataType = dataType;
        }
    }
}
