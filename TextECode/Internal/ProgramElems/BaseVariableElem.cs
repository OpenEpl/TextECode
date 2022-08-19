using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems
{
    internal abstract class BaseVariableElem : ProgramElem
    {
        public int Id { get; }
        public abstract BaseDataTypeElem DataType { get; }
        public BaseVariableElem(TextECodeRestorer p, int id) : base(p)
        {
            Id = id;
        }
    }
}
