using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems
{
    internal interface IMemberElem
    {
        public int ClassId { get; }
        public int MemberId { get; }
        public BaseDataTypeElem DataType { get; }
        public int? ImplicitTarget { get; }
    }
}
