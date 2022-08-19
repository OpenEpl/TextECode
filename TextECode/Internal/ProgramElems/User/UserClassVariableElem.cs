using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal;
using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserClassVariableElem : BaseVariableElem, IMemberElem
    {
        public EplParser.ClassVariableElemContext Tree;
        public string Name { get; }
        public override BaseDataTypeElem DataType { get; }
        public int ClassId { get; }
        public int MemberId => Id;
        public int? ImplicitTarget => null;

        public UserClassVariableElem(TextECodeRestorer p, EplParser.ClassVariableElemContext tree, int classId) : base(p, p.AllocId(EplSystemId.Type_ClassMember))
        {
            Tree = tree;
            Name = Tree.name?.Text;
            DataType = P.GetTypeElem(tree.dataType?.Text);
            ClassId = classId;
        }

        public ClassVariableInfo ToNative()
        {
            return new ClassVariableInfo(Id)
            {
                Name = Name,
                Comment = Tree.Comment()?.GetText(),
                DataType = DataType?.Id ?? 0,
                UBound = TokenUtils.ReadArrayUBound(Tree.array)
            };
        }
    }
}
