using OpenEpl.ELibInfo;
using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Utils.Scopes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.External
{
    internal class ExternalDataTypeElem : BaseDataTypeElem
    {
        public override IScope<ProgramElemName, ProgramElem> ScopeFromOuter { get; }

        public ExternalDataTypeElem(TextECodeRestorer p, int id, IScope<ProgramElemName, ProgramElem> scopeFromOuter) : base(p, id)
        {
            ScopeFromOuter = scopeFromOuter;
        }
    }
}