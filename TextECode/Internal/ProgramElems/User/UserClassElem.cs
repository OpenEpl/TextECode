using Microsoft.Extensions.Logging;
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
    internal class UserClassElem : BaseDataTypeElem
    {
        public EplParser.ClassElemContext Tree;
        public Scope<ProgramElemName, ProgramElem> SelfMemberScope;
        public Scope<ProgramElemName, ProgramElem> PrivateScope;
        public IScope<ProgramElemName, ProgramElem> ScopeFromInner;
        public List<UserClassVariableElem> Variables;
        public List<UserMethodElem> Methods;
        public string Name { get; }
        private IScope<ProgramElemName, ProgramElem> _scopeFromOuter;
        public override IScope<ProgramElemName, ProgramElem> ScopeFromOuter => _scopeFromOuter;

        public BaseDataTypeElem Base;
        public UserFormDataType FormDataType;

        public UserClassElem(TextECodeRestorer p, EplParser.ClassElemContext tree) : base(p, p.AllocId(DetectType(tree)))
        {
            Tree = tree;
            Name = Tree.name?.Text;
        }

        private static int DetectType(EplParser.ClassElemContext tree)
        {
            if (tree == null)
            {
                throw new ArgumentNullException(nameof(tree));
            }
            var type = EplSystemId.Type_StaticClass;
            if (tree.@base != null)
            {
                if (tree.@base.WinPrefix() != null)
                {
                    type = EplSystemId.Type_FormClass;
                }
                else
                {
                    type = EplSystemId.Type_Class;
                }
            }

            return type;
        }
        
        /// <remarks>
        /// This method is called with topological order.
        /// </remarks>
        public void AssociateBase()
        {
            if (Tree.@base != null)
            {
                var baseName = Tree.@base.Identifier()?.GetText();
                if (Tree.@base.WinPrefix() != null)
                {
                    try
                    {
                        FormDataType = P.FormTypeScope[baseName];
                        FormDataType.SetFormClassId(Id);
                    }
                    catch (Exception e)
                    {
                        P.translatorLogger.LogWarning(e, "无法关联窗口程序集，未找到窗口 {FormName}", baseName);
                    }
                }
                else
                {
                    Base = P.GetTypeElem(baseName);
                }
            }
        }

        /// <remarks>
        /// This method is called with topological order.
        /// </remarks>
        public void DefineAll()
        {
            SelfMemberScope = new();
            PrivateScope = new();
            if (Base?.ScopeFromOuter is not null && Base.ScopeFromOuter is not EmptyScope<ProgramElemName, ProgramElem>)
            {
                _scopeFromOuter = new InheritedScope<ProgramElemName, ProgramElem>(SelfMemberScope, Base.ScopeFromOuter);
            }
            else
            {
                if (Base is not null && Base.ScopeFromOuter is null)
                {
                    P.translatorLogger.LogWarning("无法为 {Name} 关联基类的成员", Name);
                }
                _scopeFromOuter = SelfMemberScope;
            }
            if (FormDataType != null)
            {
                ScopeFromInner = new InheritedScope<ProgramElemName, ProgramElem>(
                    new ParallelScope<ProgramElemName, ProgramElem>(new IScope<ProgramElemName, ProgramElem>[] {
                        ScopeFromOuter,
                        PrivateScope,
                        FormDataType.ScopeForForm
                    }),
                    P.TopLevelScope
                );
                FormDataType.SetScopeFromOuter(ScopeFromInner);
            }
            else
            {
                ScopeFromInner = new InheritedScope<ProgramElemName, ProgramElem>(
                    new ParallelScope<ProgramElemName, ProgramElem>(new IScope<ProgramElemName, ProgramElem>[] {
                        ScopeFromOuter,
                        PrivateScope
                    }),
                    P.TopLevelScope
                );
            }
            Variables = new();
            foreach (var item in Tree.classVariableElem())
            {
                var elem = new UserClassVariableElem(P, item, this.Id);
                Variables.Add(elem);
                if (!string.IsNullOrEmpty(elem.Name))
                {
                    PrivateScope.Add(ProgramElemName.Var(elem.Name), elem);
                }
            }
            Methods = new();
            foreach (var item in Tree.methodElem())
            {
                var elem = new UserMethodElem(P, item, this);
                elem.DefineAll();
                Methods.Add(elem);
                if (!string.IsNullOrEmpty(elem.Name))
                {
                    if (EplSystemId.GetType(Id) == EplSystemId.Type_Class)
                    {
                        SelfMemberScope.Add(ProgramElemName.Method(elem.Name), elem);
                    }
                    else
                    {
                        P.UserTopLevelScope.Add(ProgramElemName.Method(elem.Name), elem);
                    }
                }
            }
        }

        /// <remarks>
        /// Finish is not called with topological order. We keep the original order.
        /// </remarks>
        public void Finish()
        {
            using (P.translatorLogger.BeginScope($"所在类模块：{Name}"))
            {
                foreach (var item in Methods) item.Finish();
                var native = new ClassInfo(Id)
                {
                    Name = Name,
                    Form = FormDataType?.Id ?? 0,
                    Methods = Methods.Select(x => x.Id).ToList(),
                    Variables = Variables.Select(x => x.ToNative()).ToList(),
                    Comment = Tree.Comment()?.GetText(),
                };
                if (Base != null)
                {
                    native.BaseClass = Base.Id;
                }
                else
                {
                    native.BaseClass = EplSystemId.GetType(Id) == EplSystemId.Type_Class ? -1 : 0;
                }
                P.Code.Classes.Add(native);
                if (Tree.Public() != null)
                {
                    P.ClassPublicity.ClassPublicities.Add(new()
                    {
                        Class = Id,
                        Public = true
                    });
                }
            }
        }
    }
}
