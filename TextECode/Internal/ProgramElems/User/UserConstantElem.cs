using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal abstract class UserConstantElem : ProgramElem
    {
        public ConstantInfo Native { get; }
        public int Id => Native.Id;
        public string Name => Native.Name;
        public BaseDataTypeElem DataType { get; }
        public UserConstantElem(TextECodeRestorer p, ConstantInfo native) : base(p)
        {
            Native = native;

            DataType = native.Value switch
            {
                double _ => P.SystemDataTypes.Double,
                string _ => P.SystemDataTypes.String,
                byte[] _ => P.SystemDataTypes.Bin,
                bool _ => P.SystemDataTypes.Bool,
                DateTime _ => P.SystemDataTypes.DateTime,
                _ => P.SystemDataTypes.String,
            };
        }

        public void Finish()
        {
            P.Resource.Constants.Add(Native);
        }
    }
}
