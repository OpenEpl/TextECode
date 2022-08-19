using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserNormalConstantElem : UserConstantElem
    {
        public UserNormalConstantElem(TextECodeRestorer p, EplParser.ConstantElemContext tree)
            : base(p, GenerateNativeInfoFromTree(p, tree))
        {
        }

        private static ConstantInfo GenerateNativeInfoFromTree(TextECodeRestorer p, EplParser.ConstantElemContext tree)
        {
            var id = p.AllocId(EplSystemId.Type_Constant);
            var native = new ConstantInfo(id)
            {
                Name = tree.name?.Text,
                Public = tree.Public() != null,
                Comment = tree.Comment()?.GetText()
            };
            var valueContent = TokenUtils.ReadStringItem(tree.value);
            if (valueContent != null)
            {
                switch (valueContent)
                {
                    case var x when x.Length >= 2 && x[0] == '“' && x[^1] == '”':
                        native.Value = x[1..^1];
                        break;
                    case var x when x == "真" || x == "假":
                        native.Value = x == "真";
                        break;
                    case var x when double.TryParse(x, out var number):
                        native.Value = number;
                        break;
                    case var x when x.Length >= 2 && x[0] == '[' && x[^1] == ']':
                        native.Value = TokenUtils.ReadDateTimeLiteral(valueContent);
                        break;
                    default:
                        native.Value = valueContent;
                        native.Unexamined = true;
                        break;
                }
            }
            return native;
        }
    }
}
