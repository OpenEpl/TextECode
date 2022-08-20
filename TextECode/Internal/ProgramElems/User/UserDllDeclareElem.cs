using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserDllDeclareElem : BaseCallableElem
    {
        public EplParser.DllDeclareElemContext Tree;
        public int Id { get; }
        public string Name { get; }
        private BaseDataTypeElem _returnDataType;
        public override BaseDataTypeElem ReturnDataType => _returnDataType;
        private List<UserDllArgElem> Args;

        public UserDllDeclareElem(TextECodeRestorer p, EplParser.DllDeclareElemContext tree) : base(p)
        {
            Tree = tree;
            Id = P.AllocId(EplSystemId.Type_Dll);
            Name = Tree.name?.Text;
        }
        public void DefineAll()
        {
            _returnDataType = P.GetTypeElem(Tree.returnDataType?.Text);
            Args = new();
            foreach (var item in Tree.argElem())
            {
                var elem = new UserDllArgElem(P, item);
                Args.Add(elem);
            }
        }

        public void Finish()
        {
            var native = new DllDeclareInfo(Id)
            {
                Name = Name,
                LibraryName = TokenUtils.ReadStringItem(Tree.libraryName),
                EntryPoint = TokenUtils.ReadStringItem(Tree.entryPoint),
                Comment = Tree.Comment()?.GetText(),
                Public = Tree.Public() != null,
                Parameters = Args.Select(x => x.ToNative()).ToList(),
                ReturnDataType = ReturnDataType?.Id ?? 0,
            };
            P.Code.DllDeclares.Add(native);
        }
    }
}
