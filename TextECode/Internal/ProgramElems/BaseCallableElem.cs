using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems
{
    internal abstract class BaseCallableElem : ProgramElem
    {
        public BaseCallableElem(TextECodeRestorer p) : base(p)
        {
        }

        public abstract BaseDataTypeElem ReturnDataType { get; }
    }
}
