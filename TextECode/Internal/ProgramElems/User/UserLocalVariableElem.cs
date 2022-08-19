using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal;
using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserLocalVariableElem : BaseVariableElem
    {
        public EplParser.LocalVariableElemContext Tree;
        public string Name { get; }
        public override BaseDataTypeElem DataType { get; }
        public UserLocalVariableElem(TextECodeRestorer p, EplParser.LocalVariableElemContext tree) : base(p, p.AllocId(EplSystemId.Type_Local))
        {
            Tree = tree;
            Name = Tree.name?.Text;
            DataType = P.GetTypeElem(tree.dataType?.Text);
        }

        public LocalVariableInfo ToNative()
        {
            return new LocalVariableInfo(Id)
            {
                Name = Name,
                Static = Tree.Static() != null,
                Comment = Tree.Comment()?.GetText(),
                DataType = DataType?.Id ?? 0,
                UBound = TokenUtils.ReadArrayUBound(Tree.array)
            };
        }
    }
}
