using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Utils.Scopes;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserStructElem : BaseDataTypeElem
    {
        public string Name { get; }
        public EplParser.StructElemContext Tree;

        public Scope<ProgramElemName, ProgramElem> SelfMemberScope;
        public override IScope<ProgramElemName, ProgramElem> ScopeFromOuter => SelfMemberScope;
        public List<UserStructMemberElem> Members;

        public UserStructElem(TextECodeRestorer p, EplParser.StructElemContext tree) : base(p, p.AllocId(EplSystemId.Type_Struct))
        {
            Tree = tree;
            Name = Tree.name?.Text;
        }

        public void DefineAll()
        {
            Members = new();
            SelfMemberScope = new();
            foreach (var item in Tree.structMemberElem())
            {
                var elem = new UserStructMemberElem(P, item, this.Id);
                Members.Add(elem);
                if (!string.IsNullOrEmpty(elem.Name))
                {
                    SelfMemberScope.Add(ProgramElemName.Var(elem.Name), elem);
                }
            }
        }

        public void Finish()
        {
            var native = new StructInfo(Id)
            {
                Name = Name,
                Public = Tree.Public() != null,
                Comment = Tree.Comment()?.GetText(),
                Members = Members.Select(x => x.ToNative()).ToList()
            };
            P.Code.Structs.Add(native);
        }
    }
}
