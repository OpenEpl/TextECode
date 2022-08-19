using OpenEpl.TextECode.Internal.ProgramElems;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.External
{
    internal class ExternalVariableElem : BaseVariableElem
    {
        public ExternalVariableElem(TextECodeRestorer p, int id, BaseDataTypeElem dataType) : base(p, id)
        {
            DataType = dataType;
        }

        public override BaseDataTypeElem DataType { get; }
    }
}
