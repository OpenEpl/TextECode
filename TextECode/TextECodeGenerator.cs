using Microsoft.Extensions.Logging;
using MimeDetective;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenEpl.TextECode.Internal;
using OpenEpl.TextECode.Model;
using QIQI.EProjectFile;
using QIQI.EProjectFile.Sections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenEpl.TextECode
{
    public class TextECodeGenerator
    {
        private static readonly ContentInspector SharedContentInspector = new ContentInspectorBuilder()
        {
            Definitions = new MimeDetective.Definitions.CondensedBuilder()
            {
                UsageType = MimeDetective.Definitions.Licensing.UsageType.PersonalNonCommercial
            }.Build()
        }.Build();

        private readonly EplDocument Doc;
        private readonly ESystemInfoSection System;
        private readonly ProjectConfigSection ProjectConfig;
        private readonly CodeSection Code;
        private readonly ResourceSection Resource;
        private readonly ECDependenciesSection ECDependencies;
        private readonly FolderSection Folder;
        private readonly ClassPublicitySection ClassPublicity;
        private readonly Dictionary<int, MethodInfo> MethodIdMap;
        private readonly Dictionary<int, ClassInfo> ClassIdMap;
        private readonly Dictionary<int, DllDeclareInfo> DllIdMap;
        private readonly Dictionary<int, GlobalVariableInfo> GlobalVariableIdMap;
        private readonly Dictionary<int, StructInfo> StructIdMap;
        private readonly Dictionary<int, ConstantInfo> ConstantIdMap;
        private readonly Dictionary<int, FormInfo> FormIdMap;
        private readonly Dictionary<int, CodeFolderInfo> FolderKeyMap;
        private readonly Dictionary<int, FormInfo> FormClassMap;
        private readonly IdToNameMap IdToNameMap;
        public IDictionary<string, JToken> ExtensionData { get; set; }
        public string WorkingDir { get; }
        public string ProjectFilePath { get; }
        private string SrcBasePath;
        private string ProgramOutFile;
        private readonly HashSet<int> handledIds = new();
        private readonly HashSet<int> publicClassIds = new();
        private readonly HashSet<int> hiddenClassIds = new();

        public ILoggerFactory LoggerFactory { get; }

        private readonly ILogger<TextECodeGenerator> logger;
        public IEComSearcher EComSearcher { get; }
        public HashSet<string> GeneratedPaths { get; } = new();

        public TextECodeGenerator(ILoggerFactory loggerFactory, EplDocument doc, string projectFilePath, IEComSearcher ecomSearcher)
        {
            this.LoggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<TextECodeGenerator>();
            this.EComSearcher = ecomSearcher;
            this.Doc = doc;
            this.System = doc.Get(ESystemInfoSection.Key);
            this.ProjectConfig = doc.Get(ProjectConfigSection.Key);
            this.Code = doc.Get(CodeSection.Key);
            this.Resource = doc.Get(ResourceSection.Key);
            this.ECDependencies = doc.GetOrNull(ECDependenciesSection.Key);
            this.Folder = doc.GetOrNull(FolderSection.Key);
            this.ClassPublicity = doc.GetOrNull(ClassPublicitySection.Key);
            this.MethodIdMap = Code.Methods.ToDictionary(x => x.Id);
            this.ClassIdMap = Code.Classes.ToDictionary(x => x.Id);
            this.DllIdMap = Code.DllDeclares.ToDictionary(x => x.Id);
            this.GlobalVariableIdMap = Code.GlobalVariables.ToDictionary(x => x.Id);
            this.StructIdMap = Code.Structs.ToDictionary(x => x.Id);
            this.ConstantIdMap = Resource.Constants.ToDictionary(x => x.Id);
            this.FormIdMap = Resource.Forms.ToDictionary(x => x.Id);
            this.FolderKeyMap = Folder?.Folders.ToDictionary(x => x.Key);
            this.FormClassMap = Resource.Forms.Where(x => x.Class != 0).ToDictionary(x => x.Class);
            this.IdToNameMap = new IdToNameMap(doc);
            this.ProjectFilePath = Path.GetFullPath(projectFilePath);
            this.WorkingDir = Path.GetDirectoryName(this.ProjectFilePath);
            this.SrcBasePath = Path.Combine(WorkingDir, "src");
            if (File.Exists(ProjectFilePath))
            {
                try
                {
                    using var reader = new JsonTextReader(new StreamReader(File.Open(ProjectFilePath, FileMode.Open), Encoding.UTF8));
                    var projectModel = JsonSerializer.Create().Deserialize<ProjectModel>(reader);
                    this.SrcBasePath = Path.GetFullPath(projectModel.SourceSet, WorkingDir);
                    this.ProgramOutFile = Path.GetFullPath(projectModel.OutFile, WorkingDir);
                    this.ExtensionData = projectModel.ExtensionData;
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "尝试从原有项目信息文件中提取数据但失败");
                }
            }
            if (ClassPublicity is not null)
            {
                foreach (var item in ClassPublicity.ClassPublicities)
                {
                    if (item.Public)
                    {
                        publicClassIds.Add(item.Class);
                    }
                    if (item.Hidden)
                    {
                        hiddenClassIds.Add(item.Class);
                    }
                }
            }
        }
        public TextECodeGenerator(ILoggerFactory loggerFactory, EplDocument doc, string projectFilePath)
            : this(loggerFactory, doc, projectFilePath, TextECode.EComSearcher.Default)
        {

        }

        public void SetSourceSet(string sourceSet)
        {
            this.SrcBasePath = Path.GetFullPath(sourceSet, WorkingDir);
        }

        public void SetProgramOutFile(string outFile)
        {
            this.ProgramOutFile = Path.GetFullPath(outFile, WorkingDir);
        }

        public TextECodeGenerator Generate()
        {
            GeneratedPaths.Clear();
            {
                foreach (var (manifest, refInfo) in IdToNameMap.LibDefinedName.Zip(Code.Libraries, (manifest, refInfo) => (manifest, refInfo)))
                {
                    if (manifest == null)
                    {
                        logger.LogError("加载支持库 {Name} (FileName={FileName}, GUID={Guid}) v{Version} 失败",
                            refInfo.Name,
                            refInfo.FileName,
                            refInfo.GuidString,
                            refInfo.Version);
                    }
                }
            }
            handledIds.Clear();
            Directory.CreateDirectory(WorkingDir);
            GenerateProjectFile();
            if (Folder != null)
            {
                foreach (var folderInfo in Folder.Folders)
                {
                    var classes = folderInfo.Children.Where(x => EplSystemId.GetType(x) switch
                    {
                        EplSystemId.Type_Class => true, 
                        EplSystemId.Type_FormClass => true, 
                        EplSystemId.Type_StaticClass => true,
                        _ => false
                    }).Select(x => ClassIdMap[x]);
                    HandleClasses(folderInfo, classes);
                    var dlls = folderInfo.Children
                        .Where(x => EplSystemId.GetType(x) == EplSystemId.Type_Dll)
                        .Select(x => DllIdMap[x]);
                    HandleNormalObjects("@DLL", folderInfo, dlls, true, x => x.Hidden);
                    var globalVariables = folderInfo.Children
                        .Where(x => EplSystemId.GetType(x) == EplSystemId.Type_Global)
                        .Select(x => GlobalVariableIdMap[x]);
                    HandleNormalObjects("@Global", folderInfo, globalVariables, false, x => x.Hidden);
                    var structs = folderInfo.Children
                        .Where(x => EplSystemId.GetType(x) == EplSystemId.Type_Struct)
                        .Select(x => StructIdMap[x]);
                    HandleNormalObjects("@Struct", folderInfo, structs, true, x => x.Hidden);
                    var constants = folderInfo.Children
                        .Where(x => EplSystemId.GetType(x) switch
                        {
                            EplSystemId.Type_Constant => true,
                            EplSystemId.Type_ImageResource => true,
                            EplSystemId.Type_SoundResource => true,
                            _ => false
                        })
                        .Select(x => ConstantIdMap[x]);
                    HandleConstants(folderInfo, constants);
                    var forms = folderInfo.Children
                        .Where(x => EplSystemId.GetType(x) == EplSystemId.Type_Form)
                        .Select(x => FormIdMap[x]);
                    HandleForms(folderInfo, forms);
                }
            }
            HandleClasses(null, Code.Classes);
            HandleNormalObjects("@DLL", null, Code.DllDeclares, true, x => x.Hidden);
            HandleNormalObjects("@Global", null, Code.GlobalVariables, false, x => x.Hidden);
            HandleNormalObjects("@Struct", null, Code.Structs, true, x => x.Hidden);
            HandleConstants(null, Resource.Constants);
            HandleForms(null, Resource.Forms);
            return this;
        }

        private void GenerateProjectFile()
        {
            var projectModel = new ProjectModel()
            {
                Name = ProjectConfig.Name,
                ProjectType = (EplProjectType)System.ProjectType,
                Language = (EplLanguage)System.Language,
                Description = ProjectConfig.Description,
                Author = ProjectConfig.Author,
                ZipCode = ProjectConfig.ZipCode,
                Address = ProjectConfig.Address,
                TelephoneNumber = ProjectConfig.TelephoneNumber,
                FaxNumber = ProjectConfig.FaxNumber,
                Email = ProjectConfig.Email,
                Homepage = ProjectConfig.Homepage,
                Copyright = ProjectConfig.Copyright,
                Version = ProjectConfig.Version,
                WriteVersion = ProjectConfig.WriteVersion,
                CompilePlugins = ProjectConfig.CompilePlugins,
                ExportPublicClassMethod = ProjectConfig.ExportPublicClassMethod,
                SourceSet = Path.GetRelativePath(WorkingDir, SrcBasePath),
                OutFile = string.IsNullOrEmpty(ProgramOutFile) ? null : Path.GetRelativePath(WorkingDir, ProgramOutFile),
                ExtensionData = ExtensionData
            };
            IEnumerable<DependencyModel> dependencies = Code.Libraries.Select(x => new DependencyModel.ELib(
                x.Name,
                x.FileName,
                x.GuidString,
                x.Version));
            if (ECDependencies is not null)
            {
                dependencies = dependencies.Concat(ECDependencies.ECDependencies.Select(x =>
                {
                    var path = EComSearcher.Search(x.Name, x.Path) ?? x.Path;
                    path = Path.GetRelativePath(WorkingDir, path);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        path = path.Replace('\\', '/');
                    }
                    return new DependencyModel.ECom(
                        x.Name,
                        path,
                        x.ReExport);
                }));
            }
            projectModel.Dependencies = dependencies.ToList();
            using var writer = new JsonTextWriter(new StreamWriter(File.Open(ProjectFilePath, FileMode.Create), Encoding.UTF8));
            JsonSerializer.Create(new JsonSerializerSettings() { 
                Formatting = Formatting.Indented
            }).Serialize(writer, projectModel);
        }

        public TextECodeGenerator DeleteNonGeneratedFiles()
        {
            var srcBase = new DirectoryInfo(SrcBasePath);
            var NonGeneratedECodes = srcBase.GetFiles("*.ecode", SearchOption.AllDirectories)
                .Where(x => !GeneratedPaths.Contains(x.FullName));
            var NonGeneratedEForms = srcBase.GetFiles("*.eform", SearchOption.AllDirectories)
                .Where(x => !GeneratedPaths.Contains(x.FullName));
            var NonGeneratedResources = srcBase.GetDirectories("@Resource", SearchOption.AllDirectories)
                .SelectMany(x => x.EnumerateFiles())
                .Where(x => !GeneratedPaths.Contains(x.FullName));
            foreach (var item in NonGeneratedECodes.Concat(NonGeneratedEForms).Concat(NonGeneratedResources))
            {
                try
                {
                    item.Delete();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "无法删除文件 \"{Path}\"", item.FullName);
                }
            }
            var NonGeneratedEmptyDirectories = srcBase.GetDirectories("*", SearchOption.AllDirectories)
                .Where(x => !GeneratedPaths.Contains(x.FullName))
                .Where(x => x.EnumerateFiles().FirstOrDefault() == null);
            foreach (var item in NonGeneratedEmptyDirectories)
            {
                try
                {
                    item.Delete();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "无法删除目录 \"{Path}\"", item.FullName);
                }
            }
            return this;
        }

        private void HandleNormalObjects<T>(string name, CodeFolderInfo folderInfo, IEnumerable<T> objects, bool addtionalEmptyLine, Func<T, bool> isHidden)
            where T: IHasId, IToTextCodeAble
        {
            var enumerator = objects.Where(x => !isHidden(x) && handledIds.Add(x.Id)).GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return;
            }
            using var writer = CreateECodeFile(folderInfo, name);
            do
            {
                var elem = enumerator.Current;
                elem.ToTextCode(IdToNameMap, writer, 0);
                writer.WriteLine();
                if (addtionalEmptyLine)
                {
                    writer.WriteLine();
                }
            } while (enumerator.MoveNext());
        }

        private void HandleConstants(CodeFolderInfo folderInfo, IEnumerable<ConstantInfo> constants)
        {
            if (constants.FirstOrDefault(x => !x.Hidden && !handledIds.Contains(x.Id)) is null)
            {
                return;
            }
            using var writer = CreateECodeFile(folderInfo, "@Constant");
            foreach (var elem in constants.Where(x => EplSystemId.GetType(x.Id) == EplSystemId.Type_Constant))
            {
                if (elem.Hidden || !handledIds.Add(elem.Id))
                {
                    continue;
                }
                if (elem.LongText)
                {
                    var name = IdToNameMap.GetUserDefinedName(elem.Id);
                    var value = elem.Value as string ?? string.Empty;
                    WriteResourceFile(folderInfo, name, "txt", value);
                    WriteDefinitionCode(writer, 0, "长文本", name, elem.Public ? "公开" : "", elem.Comment);
                }
                else
                {
                    elem.ToTextCode(IdToNameMap, writer, 0);
                }
                writer.WriteLine();
            }
            foreach (var elem in constants.Where(x => EplSystemId.GetType(x.Id) == EplSystemId.Type_ImageResource))
            {
                if (elem.Hidden || !handledIds.Add(elem.Id))
                {
                    continue;
                }
                var name = IdToNameMap.GetUserDefinedName(elem.Id);
                var value = elem.Value as byte[];
                if (!string.IsNullOrEmpty(name) && value is not null)
                {
                    var ext = DetectFileExtension(value) ?? "dat";
                    WriteResourceFile(folderInfo, name, ext, value);
                }
                WriteDefinitionCode(writer, 0, "图片", IdToNameMap.GetUserDefinedName(elem.Id), elem.Public ? "公开" : "", elem.Comment);
                writer.WriteLine();
            }
            foreach (var elem in constants.Where(x => EplSystemId.GetType(x.Id) == EplSystemId.Type_SoundResource))
            {
                if (elem.Hidden || !handledIds.Add(elem.Id))
                {
                    continue;
                }
                var name = IdToNameMap.GetUserDefinedName(elem.Id);
                var value = elem.Value as byte[];
                if (!string.IsNullOrEmpty(name) && value is not null)
                {
                    var ext = DetectFileExtension(value) ?? "dat";
                    WriteResourceFile(folderInfo, name, ext, value);
                }
                WriteDefinitionCode(writer, 0, "声音", IdToNameMap.GetUserDefinedName(elem.Id), elem.Public ? "公开" : "", elem.Comment);
                writer.WriteLine();
            }
        }

        private void HandleClasses(CodeFolderInfo folderInfo, IEnumerable<ClassInfo> classes)
        {
            foreach (var elem in classes)
            {
                HandleClass(folderInfo, elem);
            }
        }

        private void HandleForms(CodeFolderInfo folderInfo, IEnumerable<FormInfo> classes)
        {
            foreach (var elem in classes)
            {
                HandleForm(folderInfo, elem);
            }
        }

        private void HandleForm(CodeFolderInfo folderInfo, FormInfo elem)
        {
            if (!handledIds.Add(elem.Id))
            {
                return;
            }
            using var stream = CreateEFormFile(folderInfo, elem.Name);
            new FormInfoGenerator(elem, IdToNameMap).Save(stream);
        }

        private void HandleClass(CodeFolderInfo folderInfo, ClassInfo elem)
        {
            if (hiddenClassIds.Contains(elem.Id) || !handledIds.Add(elem.Id))
            {
                return;
            }
            using var writer = CreateECodeFile(folderInfo, elem.Name);
            var isPublic = publicClassIds.Contains(elem.Id);
            if (elem.BaseClass == 0)
            {
                if (FormClassMap.TryGetValue(elem.Id, out var form))
                {
                    WriteDefinitionCode(
                        writer,
                        0,
                        "程序集",
                        IdToNameMap.GetUserDefinedName(elem.Id),
                        $"窗口: {form.Name}",
                        isPublic ? "公开" : "",
                        elem.Comment);
                }
                else
                {
                    WriteDefinitionCode(
                        writer,
                        0,
                        "程序集",
                        IdToNameMap.GetUserDefinedName(elem.Id),
                        string.Empty,
                        isPublic ? "公开" : "",
                        elem.Comment);
                }
            }
            else if (elem.BaseClass == -1)
            {
                WriteDefinitionCode(
                    writer,
                    0,
                    "程序集",
                    IdToNameMap.GetUserDefinedName(elem.Id),
                    "<对象>",
                    isPublic ? "公开" : "",
                    elem.Comment);
            }
            else
            {
                WriteDefinitionCode(
                    writer,
                    0,
                    "程序集",
                    IdToNameMap.GetUserDefinedName(elem.Id),
                    IdToNameMap.GetUserDefinedName(elem.BaseClass),
                    isPublic ? "公开" : "",
                    elem.Comment);
            }
            writer.WriteLine();
            foreach (var variableInfo in elem.Variables)
            {
                variableInfo.ToTextCode(IdToNameMap, writer, 0);
                writer.WriteLine();
            }
            foreach (var methodId in elem.Methods)
            {
                var methodInfo = MethodIdMap[methodId];
                if (methodInfo.Hidden)
                {
                    continue;
                }
                writer.WriteLine();
                methodInfo.ToTextCode(IdToNameMap, writer, 0);
            }
        }

        private DirectoryInfo PrepareFolder(CodeFolderInfo folderInfo)
        {
            var path = "";
            if (folderInfo is not null)
            {
                path = folderInfo.Name;
                var curFolderId = folderInfo.ParentKey;
                while (curFolderId != 0)
                {
                    var curFolder = FolderKeyMap[curFolderId];
                    path = $"{curFolder.Name}{Path.DirectorySeparatorChar}{path}";
                    curFolderId = curFolder.ParentKey;
                }
            }
            path = Path.Combine(SrcBasePath, path);
            var result = Directory.CreateDirectory(path);
            GeneratedPaths.Add(result.FullName);
            return result;
        }

        private Stream CreateEFormFile(CodeFolderInfo folderInfo, string name)
        {
            var path = Path.Combine(PrepareFolder(folderInfo).FullName, $"{name}.eform");
            GeneratedPaths.Add(path);
            return File.Open(path, FileMode.Create);
        }

        private TextWriter CreateECodeFile(CodeFolderInfo folderInfo, string name)
        {
            var path = Path.Combine(PrepareFolder(folderInfo).FullName, $"{name}.ecode");
            GeneratedPaths.Add(path);
            return new StreamWriter(File.Open(path, FileMode.Create), Encoding.UTF8);
        }

        private void WriteResourceFile(CodeFolderInfo folderInfo, string name, string ext, byte[] data)
        {
            var baseFolder = PrepareFolder(folderInfo).CreateSubdirectory("@Resource");
            GeneratedPaths.Add(baseFolder.FullName);
            var path = Path.Combine(baseFolder.FullName, $"{name}.{ext}");
            GeneratedPaths.Add(path);
            using var file = File.Open(path, FileMode.Create);
            file.Write(data, 0, data.Length);
        }

        private void WriteResourceFile(CodeFolderInfo folderInfo, string name, string ext, string data)
        {
            var baseFolder = PrepareFolder(folderInfo).CreateSubdirectory("@Resource");
            GeneratedPaths.Add(baseFolder.FullName);
            var path = Path.Combine(baseFolder.FullName, $"{name}.{ext}");
            GeneratedPaths.Add(path);
            using var writer = new StreamWriter(File.Open(path, FileMode.Create), Encoding.UTF8);
            writer.Write(data);
        }

        private static void WriteDefinitionCode(TextWriter writer, int indent, string type, params string[] items)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("声明类型不能为空", nameof(type));
            for (int i = 0; i < indent; i++)
                writer.Write("    ");
            writer.Write(".");
            writer.Write(type);
            if (items != null & items.Length != 0)
            {
                var count = items.Length;
                while (count > 0 && string.IsNullOrEmpty(items[count - 1]))
                    count--;
                if (count == 0) return;
                writer.Write(" ");
                writer.Write(string.Join(", ", items.Take(count)));
            }
        }

        private static string DetectFileExtension(byte[] data)
        {
            return SharedContentInspector.Inspect(data).ByFileExtension().FirstOrDefault()?.Extension;
        }
    }
}
