using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Utils;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserMethodArgElem : BaseVariableElem
    {
        public EplParser.ArgElemContext Tree;
        public string Name { get; }
        public override BaseDataTypeElem DataType { get; }
        public UserMethodArgElem(TextECodeRestorer p, EplParser.ArgElemContext tree) : base(p, p.AllocId(EplSystemId.Type_Local))
        {
            Tree = tree;
            Name = Tree.name?.Text;
            DataType = P.GetTypeElem(tree.dataType?.Text);
        }

        public MethodParameterInfo ToNative()
        {
            return new MethodParameterInfo(Id)
            {
                Name = Name,
                Comment = Tree.Comment()?.GetText(),
                DataType = DataType?.Id ?? 0,
                ArrayParameter = Tree.Array().Length > 0,
                OptionalParameter = Tree.Optional().Length > 0,
                ByRef = Tree.ByRef().Length > 0,
            };
        }
    }
}
