using OpenEpl.TextECode.Grammar;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserSoundResourceElem : UserConstantElem
    {
        public UserSoundResourceElem(TextECodeRestorer p, EplParser.SoundResourceElemContext tree, string resourcePath)
            : base(p, GenerateNativeInfoFromTree(p, tree, resourcePath))
        {
        }

        private static ConstantInfo GenerateNativeInfoFromTree(TextECodeRestorer p, EplParser.SoundResourceElemContext tree, string resourcePath)
        {
            var id = p.AllocId(EplSystemId.Type_SoundResource);
            var native = new ConstantInfo(id)
            {
                Name = tree.name?.Text,
                Public = tree.Public() != null,
                Comment = tree.Comment()?.GetText()
            };
            if (!string.IsNullOrEmpty(native.Name))
            {
                native.Value = File.ReadAllBytes(TextECodeRestorer.GetResourceFileName(resourcePath, native.Name));
            }
            return native;
        }
    }
}
