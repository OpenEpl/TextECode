using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems
{
    public abstract class ProgramElem
    {
        public readonly TextECodeRestorer P;

        public ProgramElem(TextECodeRestorer p)
        {
            P = p ?? throw new ArgumentNullException(nameof(p));
        }
    }
}
