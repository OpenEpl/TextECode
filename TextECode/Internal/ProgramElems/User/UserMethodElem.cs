using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Utils.Scopes;
using QIQI.EProjectFile;
using QIQI.EProjectFile.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserMethodElem : BaseCallableElem
    {
        public EplParser.MethodElemContext Tree;
        public int Id { get; }
        public string Name { get; }
        public UserClassElem ClassElem { get; }
        private BaseDataTypeElem _returnDataType;
        public override BaseDataTypeElem ReturnDataType => _returnDataType;
        private List<UserMethodArgElem> Args;
        private List<UserLocalVariableElem> Locals;
        public Scope<ProgramElemName, ProgramElem> LocalScope;

        public UserMethodElem(TextECodeRestorer p, EplParser.MethodElemContext tree, UserClassElem classElem) : base(p)
        {
            Tree = tree;
            Id = P.AllocId(EplSystemId.Type_Method);
            Name = Tree.name?.Text;
            ClassElem = classElem;
        }
        public void DefineAll()
        {
            _returnDataType = P.GetTypeElem(Tree.returnDataType?.Text);
            LocalScope = new();
            Args = new();
            Locals = new();
            foreach (var item in Tree.argElem())
            {
                var elem = new UserMethodArgElem(P, item);
                Args.Add(elem);
                if (!string.IsNullOrEmpty(elem.Name))
                {
                    LocalScope.Add(ProgramElemName.Var(elem.Name), elem);
                }
            }
            foreach (var item in Tree.localVariableElem())
            {
                var elem = new UserLocalVariableElem(P, item);
                Locals.Add(elem);
                if (!string.IsNullOrEmpty(elem.Name))
                {
                    LocalScope.Add(ProgramElemName.Var(elem.Name), elem);
                }
            }
        }

        public void Finish()
        {
            StatementBlock statBlock;
            using (P.translatorLogger.BeginScope($"所在子程序：{Name}"))
            {
                var ctc = new CodeTranslatorContext(P, new InheritedScope<ProgramElemName, ProgramElem>(LocalScope, ClassElem.ScopeFromInner));
                statBlock = ctc.TranslateStatementBlock(Tree.statementBlock());
            }
            if (statBlock.Count == 0)
            {
                statBlock.Add(new ExpressionStatement()); // 子程序至少需要包含一行代码
            }
            var native = new MethodInfo(Id)
            {
                Class = ClassElem.Id,
                Name = Name,
                Comment = Tree.Comment()?.GetText(),
                Public = Tree.Public() != null,
                Variables = Locals.Select(x => x.ToNative()).ToList(),
                Parameters = Args.Select(x => x.ToNative()).ToList(),
                ReturnDataType = ReturnDataType?.Id ?? 0,
                CodeData = statBlock.ToCodeData(P.ESysEncoding)
            };
            P.Code.Methods.Add(native);
        }
    }
}
