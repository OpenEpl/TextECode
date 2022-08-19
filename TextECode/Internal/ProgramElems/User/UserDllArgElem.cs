using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Utils;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserDllArgElem : BaseVariableElem
    {
        public EplParser.ArgElemContext Tree;
        public string Name { get; }
        public override BaseDataTypeElem DataType { get; }
        public UserDllArgElem(TextECodeRestorer p, EplParser.ArgElemContext tree) : base(p, p.AllocId(EplSystemId.Type_DllParameter))
        {
            Tree = tree;
            Name = Tree.name?.Text;
            DataType = P.GetTypeElem(tree.dataType?.Text);
        }

        public DllParameterInfo ToNative()
        {
            return new DllParameterInfo(Id)
            {
                Name = Name,
                Comment = Tree.Comment()?.GetText(),
                DataType = DataType?.Id ?? 0,
                ArrayParameter = Tree.Array().Length > 0,
                ByRef = Tree.ByRef().Length > 0
            };
        }
    }
}
