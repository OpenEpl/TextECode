using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal;
using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserGlobalVariableElem : BaseVariableElem
    {
        public EplParser.GlobalVariableElemContext Tree;
        public string Name { get; }
        private BaseDataTypeElem _dataType;
        public override BaseDataTypeElem DataType => _dataType;
        public UserGlobalVariableElem(TextECodeRestorer p, EplParser.GlobalVariableElemContext tree) : base(p, p.AllocId(EplSystemId.Type_Global))
        {
            Tree = tree;
            Name = Tree.name?.Text;
        }

        public void DefineAll()
        {
            _dataType = P.GetTypeElem(Tree.dataType?.Text);
        }

        public void Finish()
        {
            var native = new GlobalVariableInfo(Id)
            {
                Name = Name,
                Public = Tree.Public() != null,
                Comment = Tree.Comment()?.GetText(),
                DataType = DataType?.Id ?? 0,
                UBound = TokenUtils.ReadArrayUBound(Tree.array)
            };
            P.Code.GlobalVariables.Add(native);
        } 
    }
}
