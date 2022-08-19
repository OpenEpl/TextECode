using OpenEpl.TextECode.Internal.ProgramElems;
using QIQI.EProjectFile.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Internal.Expressions
{
    internal abstract class TecExpression
    {
        public readonly CodeTranslatorContext C;
        /// <summary>
        /// Tec不需要非常准确的类型推算，只需要能用于 AccessMemberExpression 的推算即可，因此该属性的信息并不总是可靠
        /// </summary>
        public abstract BaseDataTypeElem ResultDataType { get; }
        public TextECodeRestorer P => C.P;
        public TecExpression(CodeTranslatorContext c)
        {
            C = c ?? throw new ArgumentNullException(nameof(c));
        }
        public abstract Expression ToNative();
    }
}
