using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal;
using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserStructMemberElem : BaseVariableElem, IMemberElem
    {
        public EplParser.StructMemberElemContext Tree;
        public string Name { get; }
        public override BaseDataTypeElem DataType { get; }
        public int? ImplicitTarget => null;

        public int ClassId { get; }

        public int MemberId => Id;

        public UserStructMemberElem(TextECodeRestorer p, EplParser.StructMemberElemContext tree, int classId) : base(p, p.AllocId(EplSystemId.Type_StructMember))
        {
            Tree = tree;
            Name = Tree.name?.Text;
            DataType = P.GetTypeElem(tree.dataType?.Text);
            ClassId = classId;
        }

        public StructMemberInfo ToNative()
        {
            return new StructMemberInfo(Id)
            {
                Name = Name,
                ByRef = Tree.ByRef() != null,
                Comment = Tree.Comment()?.GetText(),
                DataType = DataType?.Id ?? 0,
                UBound = TokenUtils.ReadArrayUBound(Tree.array)
            };
        }
    }
}
