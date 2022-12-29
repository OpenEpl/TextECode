using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QIQI.EProjectFile;
using QIQI.EProjectFile.Sections;
using QIQI.EProjectFile.Expressions;
using QIQI.EProjectFile.Statements;
using OpenEpl.ELibInfo;
using System.Linq;
using OpenEpl.ELibInfo.Loader;
using Antlr4.Runtime;
using OpenEpl.TextECode.Grammar;
using Antlr4.Runtime.Tree;
using OpenEpl.TextECode.Model;
using OpenEpl.TextECode.Utils;
using OpenEpl.TextECode.Utils.Scopes;
using QuikGraph;
using QuikGraph.Algorithms;
using OpenEpl.TextECode.Internal.ProgramElems.User;
using OpenEpl.TextECode.Internal.ProgramElems.System;
using OpenEpl.TextECode.Internal.ProgramElems.External;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using OpenEpl.TextECode.Internal.ProgramElems;
using OpenEpl.TextECode.Internal;
using Microsoft.Extensions.Logging;

namespace OpenEpl.TextECode
{
    public class TextECodeRestorer
    {
        public ILoggerFactory LoggerFactory { get; }
        public string ProjectFilePath { get; }
        public string WorkingPath { get; }
        public IEComSearcher EComSearcher { get; }

        internal Encoding ESysEncoding { get; private set; }
        private string SrcBasePath;

        internal FolderSection Folder;
        internal CodeSection Code;
        internal ResourceSection Resource;
        internal LosableSection Losable;
        internal ClassPublicitySection ClassPublicity;
        internal ECDependenciesSection ECDependencies;
        internal int AllocId(int type) => Code.AllocId(type);

        internal ConcurrentDictionary<string, EplParser.StartContext> AstMap;

        internal readonly SystemDataTypesHelper SystemDataTypes;

        internal List<ELibManifest> ELibs;
        internal List<LibraryRefInfo> libraryRefInfos;

        internal List<UserClassElem> Classes;
        internal List<UserConstantElem> Constants;
        internal List<UserStructElem> Structs;
        internal List<UserGlobalVariableElem> GlobalVariables;
        internal List<UserDllDeclareElem> DllDeclares;
        internal List<UserFormDataType> Forms;
        internal Scope<string, UserFormDataType> FormTypeScope;
        internal List<UserLostDataType> LostDataTypes;

        internal IScope<ProgramElemName, ProgramElem> TopLevelScope;
        internal Scope<ProgramElemName, ProgramElem> SystemTopLevelScope;
        internal Scope<ProgramElemName, ProgramElem>[] LibTopLevelScopes;
        internal ExternalDataTypeElem[][] LibDataTypes;
        internal Scope<ProgramElemName, ProgramElem> UserTopLevelScope;
        internal Dictionary<Guid, int> ELibIndexMap;

        internal readonly ILogger<TextECodeRestorer> logger;
        internal readonly ILogger<CodeTranslatorContext> translatorLogger;

        public TextECodeRestorer(ILoggerFactory loggerFactory, string projectFilePath, IEComSearcher ecomSearcher)
        {
            this.LoggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger<TextECodeRestorer>();
            translatorLogger = loggerFactory.CreateLogger<CodeTranslatorContext>();
            ProjectFilePath = Path.GetFullPath(projectFilePath);
            WorkingPath = Path.GetDirectoryName(ProjectFilePath);
            EComSearcher = ecomSearcher;
            SystemDataTypes = new(this);
            SystemTopLevelScope = new();
            SystemDataTypes.AddToScope(SystemTopLevelScope);
        }

        public TextECodeRestorer(ILoggerFactory loggerFactory, string projectFilePath)
            : this(loggerFactory, projectFilePath, TextECode.EComSearcher.Default)
        {

        }

        public EplDocument Restore()
        {
            Classes = new();
            Constants = new();
            Structs = new();
            GlobalVariables = new();
            DllDeclares = new();
            Forms = new();
            FormTypeScope = new();
            LostDataTypes = new();

            Folder = new()
            {
                Folders = new()
            };
            Code = new()
            {
                Classes = new(),
                Methods = new(),
                GlobalVariables = new(),
                Structs = new(),
                DllDeclares = new()
            };
            Resource = new()
            {
                Forms = new(),
                Constants = new()
            };
            ClassPublicity = new()
            {
                ClassPublicities = new()
            };
            Losable = new()
            {
                RemovedDefinedItems = new()
            };
            ECDependencies = new()
            {
                ECDependencies = new()
            };

            using var reader = new JsonTextReader(new StreamReader(File.Open(ProjectFilePath, FileMode.Open), Encoding.UTF8));
            var projectModel = JsonSerializer.Create().Deserialize<ProjectModel>(reader);
            SrcBasePath = Path.Combine(WorkingPath, projectModel.SourceSet);
            var srcBase = new DirectoryInfo(SrcBasePath);

            var ecoms = projectModel.Dependencies
                .OfType<DependencyModel.ECom>()
                .Select(x =>
                {
                    try
                    {
                        var filePath = EComSearcher.Search(x.Name, Path.Combine(WorkingPath, x.Path));
                        if (filePath == null)
                        {
                            logger.LogError("模块 {Name} 未找到，文件路径：{Path}", x.Name, x.Path);
                            return (doc: null, model: x, file: null);
                        }
                        var fileInfo = new FileInfo(filePath);
                        using var ecFile = fileInfo.OpenRead();
                        var ecDoc = new EplDocument();
                        ecDoc.Load(ecFile);
                        return (doc: ecDoc, model: x, file: fileInfo);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "加载模块 {Name} 信息失败，文件路径：{Path}", x.Name, x.Path);
                        return (doc: null, model: x, file: null);
                    }
                })
                .ToList();


            libraryRefInfos = projectModel.Dependencies
                .OfType<DependencyModel.ELib>()
                .Select(x => new LibraryRefInfo()
                {
                    FileName = x.FileName,
                    Name = x.Name,
                    GuidString = x.Guid,
                    Version = x.Version
                })
                .Concat(ecoms.Where(x => x.doc != null).SelectMany(x => x.doc.Get(CodeSection.Key).Libraries))
                .GroupBy(x => Guid.Parse(x.GuidString))
                .Select(g =>
                {
                    return new LibraryRefInfo()
                    {
                        FileName = g.First().FileName,
                        Name = g.First().Name,
                        Version = g.Max(x => x.Version),
                        MinRequiredCmd = g.Max(x => x.MinRequiredCmd),
                        MinRequiredDataType = g.Max(x => x.MinRequiredDataType),
                        MinRequiredConstant = g.Max(x => x.MinRequiredConstant),
                        GuidString = g.First().GuidString
                    };
                }).ToList();

            {
                var iKrnlnLib = libraryRefInfos.FindIndex(x => string.Equals(
                    x.GuidString, 
                    "d09f2340818511d396f6aaf844c7e325", 
                    StringComparison.InvariantCultureIgnoreCase));
                if (iKrnlnLib == -1)
                {
                    libraryRefInfos.Insert(0, new LibraryRefInfo()
                    {
                        FileName = "krnln",
                        Name = "系统核心支持库",
                        GuidString = "d09f2340818511d396f6aaf844c7e325",
                        Version = new Version(5, 7)
                    });
                }
                else if (iKrnlnLib != 0)
                {
                    (libraryRefInfos[0], libraryRefInfos[iKrnlnLib]) = (libraryRefInfos[iKrnlnLib], libraryRefInfos[0]);
                }
            }

            ELibIndexMap = new();
            for (int iLib = 0; iLib < libraryRefInfos.Count; iLib++)
            {
                ELibIndexMap.Add(Guid.Parse(libraryRefInfos[iLib].GuidString), iLib);
            }

            ELibs = libraryRefInfos
                .Select(x => 
                {
                    try
                    {
                        return ELibInfoLoader.Default.Load(Guid.Parse(x.GuidString), x.FileName, x.Version);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "加载支持库 {Name} (FileName={FileName}, GUID={Guid}) v{Version} 失败",
                            x.Name,
                            x.FileName,
                            x.GuidString,
                            x.Version);
                        return null;
                    }
                })
                .ToList();

            InitLibTopLevelScope();

            var ecTopLevelScopes = new List<IScope<ProgramElemName, ProgramElem>>();
            foreach (var (ecDoc, ecModel, ecFile) in ecoms)
            {
                if (ecDoc == null)
                {
                    ECDependencies.ECDependencies.Add(new ECDependencyInfo()
                    {
                        InfoVersion = 2,
                        Name = ecModel.Name,
                        DefinedIds = new(),
                        ReExport = ecModel.ReExport,
                        Path = ecModel.Path,
                        FileSize = 0,
                        FileLastModifiedDate = DateTime.FromFileTimeUtc(0)
                    });
                    continue;
                }
                try
                {
                    ecTopLevelScopes.Add(new EComImporter(this, ecDoc, ecModel, ecFile).Import());
                }
                catch (Exception e)
                {
                    logger.LogError(e, "导入模块 {Name} 失败，文件路径：{Path}", ecModel.Name, ecModel.Path);
                    ECDependencies.ECDependencies.Add(new ECDependencyInfo()
                    {
                        InfoVersion = 2,
                        Name = ecModel.Name,
                        DefinedIds = new(),
                        ReExport = ecModel.ReExport,
                        Path = ecModel.Path,
                        FileSize = 0,
                        FileLastModifiedDate = DateTime.FromFileTimeUtc(0)
                    });
                }
            }

            UserTopLevelScope = new();
            TopLevelScope = new ParallelScope<ProgramElemName, ProgramElem>(
               new IScope<ProgramElemName, ProgramElem>[]
               {
                   SystemTopLevelScope,
                   UserTopLevelScope
               }
               .Concat(LibTopLevelScopes)
               .Concat(ecTopLevelScopes)
            );

            Code.Libraries = libraryRefInfos.ToArray();

            if (!string.IsNullOrEmpty(projectModel.Icon))
            {
                try
                {
                    Code.IconData = File.ReadAllBytes(Path.GetFullPath(projectModel.Icon, WorkingPath));
                }
                catch (Exception e)
                {
                    logger.LogError(e, "读取图标 {Icon} 失败", projectModel.Icon);
                }
            }

            var doc = new EplDocument();

            var system = GenerateSystemSection(projectModel);
            doc.Sections.Add(system);

            var projectConfig = GenerateProjectConfigSection(projectModel);
            doc.Sections.Add(projectConfig);

            ESysEncoding = doc.DetermineEncoding();

            Losable.OutFile = string.IsNullOrEmpty(projectModel.OutFile) ? string.Empty : Path.GetFullPath(projectModel.OutFile, WorkingPath);

            AstMap = new();
            {
                var astTasks = srcBase.GetFiles("*.ecode", SearchOption.AllDirectories)
                    .Select(file => (path: file.FullName, content: File.ReadAllText(file.FullName, Encoding.UTF8)));
                Parallel.ForEach(astTasks, x =>
                {
                    AstMap[x.path] = ParseAsAST(x.content, Path.GetRelativePath(SrcBasePath, x.path));
                });
            }
            HandleFolder(srcBase, 0);
            AstMap = null;

            // Stage: DefineForm
            foreach (var item in Forms) item.DefineForm();

            // Stage: AssociateBase
            foreach (var item in Classes)
            {
                item.AssociateBase();
            }
            var classGraph = new AdjacencyGraph<UserClassElem, IEdge<UserClassElem>>();
            foreach (var item in Classes)
            {
                if (item.Base is UserClassElem baseClassElem)
                {
                    classGraph.AddVerticesAndEdge(new Edge<UserClassElem>(baseClassElem, item));
                }
                else
                {
                    classGraph.AddVertex(item);
                }
            }
            var sortedClasses = classGraph.TopologicalSort();

            // Stage: DefineAll
            foreach (var item in sortedClasses) item.DefineAll();
            foreach (var item in Structs) item.DefineAll();
            foreach (var item in GlobalVariables) item.DefineAll();
            foreach (var item in DllDeclares) item.DefineAll();

            // Stage: Finish
            foreach (var item in Constants) item.Finish();
            foreach (var item in Structs) item.Finish();
            foreach (var item in sortedClasses) item.Finish();
            foreach (var item in GlobalVariables) item.Finish();
            foreach (var item in DllDeclares) item.Finish();
            foreach (var item in Forms) item.Finish();
            foreach (var item in LostDataTypes)
            {
                logger.LogWarning("<Type:{Name}> is not defined", item.Name);
                item.Finish();
            }

            doc.Sections.Add(Resource);
            doc.Sections.Add(Code);

            if (ClassPublicity.ClassPublicities.Count > 0)
            {
                doc.Sections.Add(ClassPublicity);
            }

            if (ECDependencies.ECDependencies.Count != 0)
            {
                doc.Sections.Add(ECDependencies);
            }

            doc.Sections.Add(Losable);

            if (Folder.Folders.Count != 0)
            {
                doc.Sections.Add(Folder);
            }

            return doc;
        }

        private EplParser.StartContext ParseAsAST(string content, string fileId)
        {
            var stream = new CodePointCharStream(content);
            var lexer = new EplLexer(stream);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new LoggerAntlrErrorListener<int>(LoggerFactory.CreateLogger<EplLexer>(), fileId));
            var tokens = new CommonTokenStream(lexer);
            var parser = new EplParser(tokens)
            {
                BuildParseTree = true
            };
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new LoggerAntlrErrorListener<IToken>(LoggerFactory.CreateLogger<EplParser>(), fileId));
            var tree = parser.start();
            return tree;
        }

        private EplParser.StartContext ParseAsAST(FileInfo file)
        {
            var content = File.ReadAllText(file.FullName, Encoding.UTF8);
            return ParseAsAST(content, Path.GetRelativePath(SrcBasePath, file.FullName));
        }

        private void InitLibTopLevelScope()
        {
            LibTopLevelScopes = new Scope<ProgramElemName, ProgramElem>[ELibs.Count];
            LibDataTypes = new ExternalDataTypeElem[ELibs.Count][];
            var dataTypeScopes = new Scope<ProgramElemName, ProgramElem>[ELibs.Count][];
            for (int libId = 0; libId < ELibs.Count; libId++)
            {
                LibTopLevelScopes[libId] = new Scope<ProgramElemName, ProgramElem>();
                var lib = ELibs[libId];
                if (lib is null)
                {
                    continue;
                }
                dataTypeScopes[libId] = new Scope<ProgramElemName, ProgramElem>[lib.DataTypes.Length];
                var dataTypeElems = LibDataTypes[libId] = new ExternalDataTypeElem[lib.DataTypes.Length];
                for (int iType = 0; iType < lib.DataTypes.Length; iType++)
                {
                    var dataType = lib.DataTypes[iType];
                    if (dataType.Deprecated >= ELibDeprecatedLevel.Hidden) continue;
                    var fullId = EplSystemId.MakeLibDataTypeId((short)libId, (short)iType);
                    IScope<ProgramElemName, ProgramElem> scope 
                        = dataTypeScopes[libId][iType] 
                        = new Scope<ProgramElemName, ProgramElem>();
                    if (dataType.Kind == ELibDataTypeKind.Component && (libId != 0 || iType != 0))
                    {
                        scope = new InheritedScope<ProgramElemName, ProgramElem>(scope, LibDataTypes[0][0].ScopeFromOuter);
                    }
                    var dataTypeElem = dataTypeElems[iType] = new ExternalDataTypeElem(this, fullId, scope);
                    LibTopLevelScopes[libId].Add(ProgramElemName.Type(dataType.Name), dataTypeElem);
                }
            }
            for (int libId = 0; libId < ELibs.Count; libId++)
            {
                var lib = ELibs[libId];
                if (lib == null)
                {
                    continue;
                }
                var libTopLevelScope = LibTopLevelScopes[libId];
                for (int constantId = 0; constantId < lib.Constants.Length; constantId++)
                {
                    var constant = lib.Constants[constantId];
                    var constantElem = new ExternalConstantElem(this, libId, constantId, constant.Name, FindTypeByLibAngle(libId, constant.DataType));
                    libTopLevelScope.Add(ProgramElemName.Constant(constantElem.Name), constantElem);
                }
                var methodElems = new ExternalMethodElem[lib.Cmds.Length];
                for (int methodId = 0; methodId < lib.Cmds.Length; methodId++)
                {
                    var cmd = lib.Cmds[methodId];
                    if (cmd.Deprecated >= ELibDeprecatedLevel.Hidden) continue;
                    methodElems[methodId] = new ExternalMethodElem(this, libId, methodId, cmd.Name, FindTypeByLibAngle(libId, cmd.ReturnDataType));
                }
                for (int iType = 0; iType < lib.DataTypes.Length; iType++)
                {
                    var dataType = lib.DataTypes[iType];
                    var dataTypeElem = LibDataTypes[libId][iType];
                    if (dataTypeElem == null) continue;
                    var dataTypeScope = dataTypeScopes[libId][iType];
                    var classMethods = dataType.Methods.Select(x => methodElems[x]).Where(x => x != null);
                    foreach (var item in classMethods)
                    {
                        dataTypeScope.Add(ProgramElemName.Method(item.Name), item);
                    }
                    for (int memberId = 0; memberId < dataType.Members.Length; memberId++)
                    {
                        var member = dataType.Members[memberId];
                        if (member.Deprecated >= ELibDeprecatedLevel.Hidden) continue;
                        var memberElem = new ExternalMemberElem(this, dataTypeElem.Id, memberId, member.Name, FindTypeByLibAngle(libId, member.DataType));
                        if (dataType.Kind == ELibDataTypeKind.Enum)
                        {
                            dataTypeScope.Add(ProgramElemName.Constant(memberElem.Name), memberElem);
                        }
                        else
                        {
                            dataTypeScope.Add(ProgramElemName.Var(memberElem.Name), memberElem);
                        }
                    }
                }
                for (int methodId = 0; methodId < lib.Cmds.Length; methodId++)
                {
                    var cmd = lib.Cmds[methodId];
                    if (cmd.CategoryId == -1) continue;
                    var methodElem = methodElems[methodId];
                    if (methodElem == null) continue;
                    libTopLevelScope.Add(ProgramElemName.Method(methodElem.Name), methodElem);
                }
            }
        }

        internal BaseDataTypeElem FindTypeByLibAngle(int libId, int dataTypeId)
        {
            if (dataTypeId == 0)
            {
                return null;
            }
            if (SystemDataTypes.TryGetById(dataTypeId, out var systemDataTypeElem))
            {
                return systemDataTypeElem;
            }
            var dataTypeElems = LibDataTypes[libId];
            if (dataTypeId <= dataTypeElems.Length)
            {
                return LibDataTypes[libId][dataTypeId - 1];
            }
            else if ((dataTypeId & 0xFFFF0000) == 0x00010000)
            {
                return LibDataTypes[0][(dataTypeId & 0x0000FFFF) - 1];
            }
            else
            {
                return LibDataTypes[0][dataTypeId - dataTypeElems.Length - 1];
            }
        }

        private static ESystemInfoSection GenerateSystemSection(ProjectModel projectModel)
        {
            return new ESystemInfoSection()
            {
                EProjectFormatVersion = new Version(1, 7),
                ESystemVersion = new Version(5, 7),
                FileType = 1,
                ProjectType = (int)projectModel.ProjectType,
                Language = (int)projectModel.Language
            };
        }

        private ProjectConfigSection GenerateProjectConfigSection(ProjectModel projectModel)
        {
            return new ProjectConfigSection()
            {
                Name = projectModel.Name,
                Description = projectModel.Description,
                Author = projectModel.Author,
                ZipCode = projectModel.ZipCode,
                Address = projectModel.Address,
                TelephoneNumber = projectModel.TelephoneNumber,
                FaxNumber = projectModel.FaxNumber,
                Email = projectModel.Email,
                Homepage = projectModel.Homepage,
                Copyright = projectModel.Copyright,
                Version = projectModel.Version,
                WriteVersion = projectModel.WriteVersion,
                CompilePlugins = projectModel.CompilePlugins ?? string.Empty,
                ExportPublicClassMethod = projectModel.ExportPublicClassMethod,
            };
        }

        private List<int> HandleFolder(DirectoryInfo directory, int folderId)
        {
            var ids = new List<int>();
            foreach (var item in directory.EnumerateDirectories())
            {
                if (item.Name.StartsWith("@"))
                {
                    continue;
                }
                var subFolderId = Folder.AllocKey();
                var subFolder = new CodeFolderInfo(subFolderId)
                {
                    Name = item.Name,
                    ParentKey = folderId,
                    Expand = true
                };
                Folder.Folders.Add(subFolder);
                var subIds = HandleFolder(item, folderId);
                subFolder.Children = subIds.ToArray();
            }
            foreach (var item in directory.EnumerateFiles("*.eform"))
            {
                var formInfoRestorer = new FormInfoRestorer(this);
                using (var stream = item.OpenRead())
                {
                    formInfoRestorer.Load(stream);
                }
                var restoredForm = formInfoRestorer.Restore();
                ids.Add(restoredForm.Id);
            }
            foreach (var item in directory.EnumerateFiles("*.ecode"))
            {
                HandleECodeFile(ids, item);
            }
            return ids;
        }

        private void HandleECodeFile(List<int> ids, FileInfo file)
        {
            if (!AstMap.TryGetValue(file.FullName, out var tree))
            {
                tree = ParseAsAST(file);
            }
            tree.Accept(new TopLevelElemDefiner(this, ids, file));
        }

        internal BaseDataTypeElem GetTypeElem(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            var key = ProgramElemName.Type(name);
            try
            {
                return (BaseDataTypeElem)TopLevelScope[key];
            }
            catch (Exception)
            {
                var result = new UserLostDataType(this, name);
                UserTopLevelScope.Add(ProgramElemName.Type(name), result);
                LostDataTypes.Add(result);
                return result;
            }
        }

        internal static string GetResourceFileName(string resourcePath, string name)
        {
            var fileName = Path.Combine(resourcePath, name);
            if (!File.Exists(fileName))
            {
                fileName = Directory.GetFiles(resourcePath, $"{name}.*").First();
            }
            return fileName;
        }
    }
}
