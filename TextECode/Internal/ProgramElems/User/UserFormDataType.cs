using OpenEpl.ELibInfo;
using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Internal.ProgramElems.External;
using OpenEpl.TextECode.Utils.Scopes;
using QIQI.EProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenEpl.TextECode.Internal.ProgramElems.User
{
    internal class UserFormDataType : BaseDataTypeElem
    {
        public IScope<ProgramElemName, ProgramElem> ScopeForForm;
        private IScope<ProgramElemName, ProgramElem> _scopeFromOuter;
        public override IScope<ProgramElemName, ProgramElem> ScopeFromOuter => _scopeFromOuter;
        private readonly FormInfo formInfo;
        private readonly FormControlInfo formSelfControl;

        public UserFormDataType(TextECodeRestorer p, FormInfo formInfo) : base(p, formInfo.Id)
        {
            this.formInfo = formInfo;
            this.formSelfControl = (FormControlInfo)formInfo.Elements.Single(x => EplSystemId.GetType(x.Id) == EplSystemId.Type_FormSelf);
        }

        public void DefineForm()
        {
            var scope = new Scope<ProgramElemName, ProgramElem>();
            foreach (var item in formInfo.Elements)
            {
                if (!string.IsNullOrEmpty(item.Name))
                {
                    EplSystemId.DecomposeLibDataTypeId(item.DataType, out var lib, out var type);
                    var dataType = P.LibDataTypes[lib][type];
                    scope.Add(ProgramElemName.Var(item.Name), new UserFormElement(P, item.Name, formInfo.Id, item.Id, dataType));
                }
            }
            var krnlnLib = P.ELibs[0];
            var formLibType = krnlnLib.DataTypes[0];
            for (int memberId = 0; memberId < formLibType.Members.Length; memberId++)
            {
                var member = formLibType.Members[memberId];
                var memberElem = new ExternalMemberElem(P, formInfo.Id, memberId + 1 /* 必须要+1 */, member.Name, P.FindTypeByLibAngle(0, member.DataType), formSelfControl.Id);
                scope.Add(ProgramElemName.Var(member.Name), memberElem);
            }
            ScopeForForm = new InheritedScope<ProgramElemName, ProgramElem>(scope, P.LibDataTypes[0][0].ScopeFromOuter);
            _scopeFromOuter = ScopeForForm;
            if (!string.IsNullOrEmpty(formInfo.Name))
            {
                P.UserTopLevelScope.Add(ProgramElemName.Var(formInfo.Name), new ExternalVariableElem(P, formInfo.Id, this));
                P.FormTypeScope.Add(formInfo.Name, this);
            }
        }

        public void SetFormClassId(int classId)
        {
            formInfo.Class = classId;
        }

        public void SetScopeFromOuter(IScope<ProgramElemName, ProgramElem> scopeFromOuter)
        {
            _scopeFromOuter = scopeFromOuter;
        }

        public void Finish()
        {
            P.Resource.Forms.Add(formInfo);
        }
    }
}
