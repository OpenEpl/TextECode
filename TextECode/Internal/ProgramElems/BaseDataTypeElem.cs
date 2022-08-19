using OpenEpl.TextECode.Utils.Scopes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems
{
    internal abstract class BaseDataTypeElem : ProgramElem
    {
        public int Id { get; }
        public virtual IScope<ProgramElemName, ProgramElem> ScopeFromOuter => EmptyScope<ProgramElemName, ProgramElem>.Instance;
        public BaseDataTypeElem(TextECodeRestorer p, int id) : base(p)
        {
            Id = id;
        }
    }
}
