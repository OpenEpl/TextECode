using OpenEpl.TextECode.Grammar;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserTextResourceElem : UserConstantElem
    {
        public UserTextResourceElem(TextECodeRestorer p, EplParser.TextResourceElemContext tree, string resourcePath)
            : base(p, GenerateNativeInfoFromTree(p, tree, resourcePath))
        {
        }

        private static ConstantInfo GenerateNativeInfoFromTree(TextECodeRestorer p, EplParser.TextResourceElemContext tree, string resourcePath)
        {
            var id = p.AllocId(EplSystemId.Type_Constant);
            var native = new ConstantInfo(id)
            {
                Name = tree.name?.Text,
                Public = tree.Public() != null,
                Comment = tree.Comment()?.GetText(),
                LongText = true
            };
            if (!string.IsNullOrEmpty(native.Name))
            {
                native.Value = File.ReadAllText(TextECodeRestorer.GetResourceFileName(resourcePath, native.Name), Encoding.UTF8);
            }
            return native;
        }
    }
}
