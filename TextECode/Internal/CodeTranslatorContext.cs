using Microsoft.Extensions.Logging;
using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Utils.Scopes;
using QIQI.EProjectFile.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenEpl.TextECode.Internal
{
    internal class CodeTranslatorContext
    {
        public readonly TextECodeRestorer P;
        public readonly IScope<ProgramElemName, ProgramElem> Scope;
        public readonly StatementTranslator StatTrans;
        public readonly ExpressionTranslator ExprTrans;

        public CodeTranslatorContext(TextECodeRestorer p, IScope<ProgramElemName, ProgramElem> scope)
        {
            P = p ?? throw new ArgumentNullException(nameof(p));
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            StatTrans = new(this);
            ExprTrans = new(this);
        }

        public StatementBlock TranslateStatementBlock(EplParser.StatementBlockContext tree)
        {
            if (tree == null)
            {
                return null;
            }
            var block = new StatementBlock();
            foreach (var item in tree.statement())
            {
                try
                {
                    block.Add(item.Accept(StatTrans));
                }
                catch (Exception e)
                {
                    var lines = TokenUtils.GetRawText(item);
                    using (StringReader reader = new(lines))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            block.Add(new UnexaminedStatement()
                            {
                                UnexaminedCode = line,
                                Mask = false
                            });
                        }
                    }
                    P.translatorLogger.LogWarning(e, "预编译代码 \"{LineText}\" 失败", lines);
                }
            }
            return block;
        }
    }
}
