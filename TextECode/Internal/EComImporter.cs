using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Model;
using OpenEpl.TextECode.Internal.ProgramElems.External;
using OpenEpl.TextECode.Utils.Scopes;
using QIQI.EProjectFile;
using QIQI.EProjectFile.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenEpl.TextECode.Internal
{
    internal class EComImporter
    {
        private readonly TextECodeRestorer P;
        private readonly EplDocument ecDoc;
        private readonly Scope<ProgramElemName, ProgramElem> ecTopScope;
        private readonly CodeSection ecCode;
        private readonly ResourceSection ecResource;
        private readonly ProjectConfigSection ecProjectConfig;
        private readonly ClassPublicitySection ecClassPublicity;
        private readonly HashSet<int> ecPublicClassIds;
        private readonly int[] ecLibIdMap;
        private readonly Dictionary<int, MethodInfo> ecMethodMap;
        private readonly Dictionary<int, ClassInfo> ecClassMap;
        private readonly List<(StructInfo dest, StructInfo src, Scope<ProgramElemName, ProgramElem> scope)> structPairs;
        private readonly List<(ClassInfo dest, ClassInfo src, Scope<ProgramElemName, ProgramElem> scope)> classPairs;
        private readonly List<(DllDeclareInfo dest, DllDeclareInfo src)> dllPairs;
        private readonly List<(MethodInfo dest, MethodInfo src)> methodPairs;
        private readonly ECDependencyInfo dependencyInfo;
        private readonly Dictionary<int, ExternalDataTypeElem> ecDataType;
        private bool completed = false;

        public EComImporter(TextECodeRestorer p, EplDocument ecDoc, DependencyModel.ECom model, FileInfo file)
        {
            P = p ?? throw new ArgumentNullException(nameof(p));
            this.ecDoc = ecDoc ?? throw new ArgumentNullException(nameof(ecDoc));
            ecTopScope = new Scope<ProgramElemName, ProgramElem>();
            ecCode = ecDoc.Get(CodeSection.Key);
            ecResource = ecDoc.Get(ResourceSection.Key);
            ecProjectConfig = ecDoc.Get(ProjectConfigSection.Key);
            ecClassPublicity = ecDoc.GetOrNull(ClassPublicitySection.Key);
            ecPublicClassIds = new HashSet<int>();
            ecLibIdMap = ecCode.Libraries.Select(x => P.ELibIndexMap[GuidUtils.ParseGuidLosely(x.GuidString)]).ToArray();
            ecMethodMap = ecCode.Methods.ToDictionary(x => x.Id);
            ecClassMap = ecCode.Classes.ToDictionary(x => x.Id);
            structPairs = new List<(StructInfo dest, StructInfo src, Scope<ProgramElemName, ProgramElem> scope)>();
            classPairs = new List<(ClassInfo dest, ClassInfo src, Scope<ProgramElemName, ProgramElem> scope)>();
            dllPairs = new List<(DllDeclareInfo dest, DllDeclareInfo src)>();
            methodPairs = new List<(MethodInfo dest, MethodInfo src)>();
            dependencyInfo = new ECDependencyInfo()
            {
                InfoVersion = 2,
                Name = ecProjectConfig.Name,
                DefinedIds = new(),
                ReExport = model.ReExport,
                Path = file.FullName,
                FileSize = (int)file.Length,
                FileLastModifiedDate = file.LastWriteTime
            };
            if (ecClassPublicity is not null)
            {
                foreach (var item in ecClassPublicity.ClassPublicities)
                {
                    if (item.Public)
                    {
                        ecPublicClassIds.Add(item.Class);
                    }
                }
            }
            ecDataType = new Dictionary<int, ExternalDataTypeElem>();
        }

        public IScope<ProgramElemName, ProgramElem> Import()
        {
            if (completed)
            {
                throw new Exception($"{nameof(EComImporter)}.{nameof(Import)} can just be called once");
            }
            completed = true;
            DefineStructs_Incomplete();
            DefineClasses_Incomplete(out var hiddenTmpMod);
            DefineGlobalVariables();
            DefineConstants();
            DefineDllDeclares_Incomplete();
            DefineMethods_Incomplete(hiddenTmpMod);
            CompleteStructs();
            CompleteDllDecalres();
            CompleteMethods();
            P.ECDependencies.ECDependencies.Add(dependencyInfo);
            return ecTopScope;
        }

        private void CompleteMethods()
        {
            foreach (var (dest, src) in methodPairs)
            {
                dest.Parameters = new(src.Parameters.Count);
                foreach (var item in src.Parameters)
                {
                    var id = P.AllocId(EplSystemId.Type_Local);
                    var typeElem = FromECType(item.DataType);
                    dest.Parameters.Add(new MethodParameterInfo(id)
                    {
                        Name = item.Name,
                        Comment = item.Comment,
                        DataType = typeElem?.Id ?? 0,
                        ByRef = item.ByRef,
                        ArrayParameter = item.ArrayParameter,
                        OptionalParameter = item.OptionalParameter
                    });
                }
            }
        }

        private void CompleteDllDecalres()
        {
            foreach (var (dest, src) in dllPairs)
            {
                dest.Parameters = new(src.Parameters.Count);
                foreach (var item in src.Parameters)
                {
                    var id = P.AllocId(EplSystemId.Type_DllParameter);
                    var typeElem = FromECType(item.DataType);
                    dest.Parameters.Add(new DllParameterInfo(id)
                    {
                        Name = item.Name,
                        Comment = item.Comment,
                        DataType = typeElem?.Id ?? 0,
                        ByRef = item.ByRef,
                        ArrayParameter = item.ArrayParameter
                    });
                }
            }
        }

        private void CompleteStructs()
        {
            foreach (var (dest, src, scope) in structPairs)
            {
                dest.Members = new(src.Members.Count);
                foreach (var item in src.Members)
                {
                    var id = P.AllocId(EplSystemId.Type_StructMember);
                    var typeElem = FromECType(item.DataType);
                    var elem = new ExternalMemberElem(P, dest.Id, id, item.Name, typeElem);
                    scope.Add(ProgramElemName.Var(item.Name), elem);
                    dest.Members.Add(new StructMemberInfo(id)
                    {
                        Name = item.Name,
                        Comment = item.Comment,
                        DataType = typeElem?.Id ?? 0,
                        ByRef = item.ByRef,
                        UBound = item.UBound
                    });
                }
            }
        }

        private void DefineMethods_Incomplete(ClassInfo hiddenTmpMod)
        {
            int idStart = 0, idCount = 0;
            foreach (var item in ecCode.Methods)
            {
                if (item.IsStatic && item.Public)
                {
                    var id = P.AllocId(EplSystemId.Type_Method);
                    if (idStart == 0)
                    {
                        idStart = id;
                    }
                    idCount++;
                    var resultTypeElem = FromECType(item.ReturnDataType);
                    var elem = new ExternalMethodElem(P, -2, id, item.Name, resultTypeElem);
                    ecTopScope.Add(ProgramElemName.Method(item.Name), elem);
                    var info = new MethodInfo(id)
                    {
                        Name = item.Name,
                        Comment = item.Comment,
                        ReturnDataType = resultTypeElem?.Id ?? 0,
                        Class = hiddenTmpMod.Id,
                        Hidden = true
                    };
                    P.Code.Methods.Add(info);
                    methodPairs.Add((info, item));
                    hiddenTmpMod.Methods.Add(id);
                }
            }
            foreach (var (dest, src, scope) in classPairs)
            {
                var methods = src.Methods.Select(x => ecMethodMap[x]);
                var baseClassId = src.BaseClass;
                while (baseClassId != 0 && baseClassId != -1)
                {
                    var baseClass = ecClassMap[src.BaseClass];
                    methods = methods.Concat(baseClass.Methods.Select(x => ecMethodMap[x]));
                    baseClassId = baseClass.BaseClass;
                }
                dest.Methods = new();
                foreach (var item in methods)
                {
                    var methodKey = ProgramElemName.Method(item.Name);
                    if (scope.GetExistence(methodKey) != KeyExistence.NotFound)
                    {
                        continue;
                    }
                    if (item.Name == "_初始化" || item.Name == "_销毁")
                    {
                        continue;
                    }
                    var id = P.AllocId(EplSystemId.Type_Method);
                    if (idStart == 0)
                    {
                        idStart = id;
                    }
                    idCount++;
                    var resultTypeElem = FromECType(item.ReturnDataType);
                    if (item.Public)
                    {
                        var elem = new ExternalMethodElem(P, -2, id, item.Name, resultTypeElem);
                        scope.Add(methodKey, elem);
                    }
                    var info = new MethodInfo(id)
                    {
                        Name = item.Name,
                        Comment = item.Comment,
                        ReturnDataType = resultTypeElem?.Id ?? 0,
                        Class = dest.Id,
                        Public = item.Public,
                        Hidden = true
                    };
                    P.Code.Methods.Add(info);
                    methodPairs.Add((info, item));
                    dest.Methods.Add(id);
                }
            }
            AddPackedIdsToECDependency(idStart, idCount);
        }

        private void DefineDllDeclares_Incomplete()
        {
            int idStart = 0, idCount = 0;
            foreach (var item in ecCode.DllDeclares)
            {
                if (item.Public)
                {
                    var id = P.AllocId(EplSystemId.Type_Dll);
                    if (idStart == 0)
                    {
                        idStart = id;
                    }
                    idCount++;
                    var resultTypeElem = FromECType(item.ReturnDataType);
                    var elem = new ExternalMethodElem(P, -3, id, item.Name, resultTypeElem);
                    ecTopScope.Add(ProgramElemName.Method(item.Name), elem);
                    var info = new DllDeclareInfo(id)
                    {
                        Name = item.Name,
                        Comment = item.Comment,
                        ReturnDataType = resultTypeElem?.Id ?? 0,
                        LibraryName = item.LibraryName,
                        EntryPoint = item.EntryPoint,
                        Hidden = true
                    };
                    P.Code.DllDeclares.Add(info);
                    dllPairs.Add((info, item));
                }
            }
            AddPackedIdsToECDependency(idStart, idCount);
        }

        private void DefineConstants()
        {
            int idStart = 0, idCount = 0;
            foreach (var item in ecResource.Constants)
            {
                if (item.Public)
                {
                    var id = P.AllocId(EplSystemId.GetType(item.Id));
                    if (idStart == 0)
                    {
                        idStart = id;
                    }
                    idCount++;
                    var typeElem = item.Value switch
                    {
                        double _ => FromECType(EplSystemId.DataType_Double),
                        string _ => FromECType(EplSystemId.DataType_String),
                        byte[] _ => FromECType(EplSystemId.DataType_Bin),
                        bool _ => FromECType(EplSystemId.DataType_Bool),
                        DateTime _ => FromECType(EplSystemId.DataType_DateTime),
                        _ => FromECType(EplSystemId.DataType_String)
                    };
                    var elem = new ExternalConstantElem(P, -2, id, item.Name, typeElem);
                    ecTopScope.Add(ProgramElemName.Constant(item.Name), elem);
                    P.Resource.Constants.Add(new ConstantInfo(id)
                    {
                        Name = item.Name,
                        Comment = item.Comment,
                        Value = item.Value,
                        LongText = item.LongText,
                        Unexamined = item.Unexamined,
                        Hidden = true
                    });
                }
            }
            AddPackedIdsToECDependency(idStart, idCount);
        }

        private void DefineGlobalVariables()
        {
            int idStart = 0, idCount = 0;
            foreach (var item in ecCode.GlobalVariables)
            {
                if (item.Public)
                {
                    var id = P.AllocId(EplSystemId.Type_Global);
                    if (idStart == 0)
                    {
                        idStart = id;
                    }
                    idCount++;
                    var typeElem = FromECType(item.DataType);
                    var elem = new ExternalVariableElem(P, id, typeElem);
                    ecTopScope.Add(ProgramElemName.Var(item.Name), elem);
                    P.Code.GlobalVariables.Add(new GlobalVariableInfo(id)
                    {
                        Name = item.Name,
                        Comment = item.Comment,
                        UBound = item.UBound,
                        DataType = typeElem.Id,
                        Hidden = true
                    });
                }
            }
            AddPackedIdsToECDependency(idStart, idCount);
        }

        private BaseDataTypeElem FromECType(int id)
        {
            if (id == 0)
            {
                return null;
            }
            if (P.SystemDataTypes.TryGetById(id, out var systemDataTypeElem))
            {
                return systemDataTypeElem;
            }
            if (EplSystemId.IsLibDataType(id))
            {
                EplSystemId.DecomposeLibDataTypeId(id, out var lib, out var type);
                return P.LibDataTypes[ecLibIdMap[lib]][type];
            }
            else
            {
                ecDataType.TryGetValue(id, out var result);
                return result;
            }
        }

        private void DefineClasses_Incomplete(out ClassInfo hiddenTmpMod)
        {
            int idStart = 0, idCount = 0;
            {
                var id = P.AllocId(EplSystemId.Type_StaticClass);
                if (idStart == 0)
                {
                    idStart = id;
                }
                idCount++;
                hiddenTmpMod = new ClassInfo(id)
                {
                    Name = "__HIDDEN_TEMP_MOD__",
                    Methods = new()
                };
                P.Code.Classes.Add(hiddenTmpMod);
                P.ClassPublicity.ClassPublicities.Add(new ClassPublicityInfo()
                {
                    Class = hiddenTmpMod.Id,
                    Hidden = true
                });
            }

            foreach (var item in ecCode.Classes)
            {
                if (EplSystemId.GetType(item.Id) == EplSystemId.Type_Class
                    && ecPublicClassIds.Contains(item.Id))
                {
                    var id = P.AllocId(EplSystemId.Type_Class);
                    if (idStart == 0)
                    {
                        idStart = id;
                    }
                    idCount++;
                    var scope = new Scope<ProgramElemName, ProgramElem>();
                    var elem = new ExternalDataTypeElem(P, id, scope);
                    ecDataType.Add(item.Id, elem);
                    ecTopScope.Add(ProgramElemName.Type(item.Name), elem);
                    var info = new ClassInfo(id)
                    {
                        Name = item.Name,
                        Comment = item.Comment,
                        BaseClass = -1 // 导出易模块时不写入继承关系，与易语言IDE做法一致
                    };
                    P.Code.Classes.Add(info);
                    P.ClassPublicity.ClassPublicities.Add(new ClassPublicityInfo()
                    {
                        Class = info.Id,
                        Hidden = true
                    });
                    classPairs.Add((info, item, scope));
                }
            }
            AddPackedIdsToECDependency(idStart, idCount);
        }

        private void DefineStructs_Incomplete()
        {
            int idStart = 0, idCount = 0;
            foreach (var item in ecCode.Structs)
            {
                if (item.Public)
                {
                    var id = P.AllocId(EplSystemId.Type_Struct);
                    if (idStart == 0)
                    {
                        idStart = id;
                    }
                    idCount++;
                    var scope = new Scope<ProgramElemName, ProgramElem>();
                    var elem = new ExternalDataTypeElem(P, id, scope);
                    ecDataType.Add(item.Id, elem);
                    ecTopScope.Add(ProgramElemName.Type(item.Name), elem);
                    var info = new StructInfo(id)
                    {
                        Name = item.Name,
                        Comment = item.Comment,
                        Hidden = true
                    };
                    P.Code.Structs.Add(info);
                    structPairs.Add((info, item, scope));
                }
            }
            AddPackedIdsToECDependency(idStart, idCount);
        }

        private void AddPackedIdsToECDependency(int idStart, int idCount)
        {
            if (idCount != 0)
            {
                dependencyInfo.DefinedIds.Add(new ECDependencyInfo.PackedIds()
                {
                    Start = idStart,
                    Count = idCount
                });
            }
        }
    }
}
