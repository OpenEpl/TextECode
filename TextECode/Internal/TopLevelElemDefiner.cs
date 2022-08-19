using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Misc;
using OpenEpl.TextECode.Grammar;
using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Internal.ProgramElems.User;
using QIQI.EProjectFile;

namespace OpenEpl.TextECode.Internal
{
    internal class TopLevelElemDefiner : EplParserBaseVisitor<int>
    {
        private readonly TextECodeRestorer P;
        private readonly List<int> ids;
        private readonly string resourcePath;

        public TopLevelElemDefiner(TextECodeRestorer p, List<int> ids, FileInfo file)
        {
            P = p ?? throw new ArgumentNullException(nameof(p));
            this.ids = ids ?? throw new ArgumentNullException(nameof(ids));
            resourcePath = Path.Combine(file.DirectoryName, "@Resource");
        }

        public override int VisitClassElem([NotNull] EplParser.ClassElemContext context)
        {
            var elem = new UserClassElem(P, context);
            P.Classes.Add(elem);
            if (!string.IsNullOrEmpty(elem.Name))
            {
                if (EplSystemId.GetType(elem.Id) == EplSystemId.Type_Class)
                {
                    P.UserTopLevelScope.Add(ProgramElemName.Type(elem.Name), elem);
                }
            }
            ids.Add(elem.Id);
            return default;
        }

        public override int VisitConstantElem([NotNull] EplParser.ConstantElemContext context)
        {
            var elem = new UserNormalConstantElem(P, context);
            P.Constants.Add(elem);
            if (!string.IsNullOrEmpty(elem.Name))
            {
                P.UserTopLevelScope.Add(ProgramElemName.Constant(elem.Name), elem);
            }
            ids.Add(elem.Id);
            return default;
        }

        public override int VisitDllDeclareElem([NotNull] EplParser.DllDeclareElemContext context)
        {
            var elem = new UserDllDeclareElem(P, context);
            P.DllDeclares.Add(elem);
            if (!string.IsNullOrEmpty(elem.Name))
            {
                P.UserTopLevelScope.Add(ProgramElemName.Method(elem.Name), elem);
            }
            ids.Add(elem.Id);
            return default;
        }

        public override int VisitGlobalVariableElem([NotNull] EplParser.GlobalVariableElemContext context)
        {
            var elem = new UserGlobalVariableElem(P, context);
            P.GlobalVariables.Add(elem);
            if (!string.IsNullOrEmpty(elem.Name))
            {
                P.UserTopLevelScope.Add(ProgramElemName.Var(elem.Name), elem);
            }
            ids.Add(elem.Id);
            return default;
        }

        public override int VisitImageResourceElem([NotNull] EplParser.ImageResourceElemContext context)
        {
            var elem = new UserImageResourceElem(P, context, resourcePath);
            P.Constants.Add(elem);
            if (!string.IsNullOrEmpty(elem.Name))
            {
                P.UserTopLevelScope.Add(ProgramElemName.Constant(elem.Name), elem);
            }
            ids.Add(elem.Id);
            return default;
        }

        public override int VisitSoundResourceElem([NotNull] EplParser.SoundResourceElemContext context)
        {
            var elem = new UserSoundResourceElem(P, context, resourcePath);
            P.Constants.Add(elem);
            if (!string.IsNullOrEmpty(elem.Name))
            {
                P.UserTopLevelScope.Add(ProgramElemName.Constant(elem.Name), elem);
            }
            ids.Add(elem.Id);
            return default;
        }

        public override int VisitStructElem([NotNull] EplParser.StructElemContext context)
        {
            var elem = new UserStructElem(P, context);
            P.Structs.Add(elem);
            if (!string.IsNullOrEmpty(elem.Name))
            {
                P.UserTopLevelScope.Add(ProgramElemName.Type(elem.Name), elem);
            }
            ids.Add(elem.Id);
            return default;
        }

        public override int VisitTextResourceElem([NotNull] EplParser.TextResourceElemContext context)
        {
            var elem = new UserTextResourceElem(P, context, resourcePath);
            P.Constants.Add(elem);
            if (!string.IsNullOrEmpty(elem.Name))
            {
                P.UserTopLevelScope.Add(ProgramElemName.Constant(elem.Name), elem);
            }
            ids.Add(elem.Id);
            return default;
        }
    }
}
