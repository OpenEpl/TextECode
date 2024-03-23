using System;
using System.Text.Json.Serialization;

namespace OpenEpl.TextECode.Model
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Kind")]
    [JsonDerivedType(typeof(ELib), "ELib")]
    [JsonDerivedType(typeof(ECom), "ECom")]
    public class DependencyModel
    {
        public class ELib : DependencyModel
        {
            public ELib(string name, string fileName, string guid, Version version)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
                Guid = guid ?? throw new ArgumentNullException(nameof(guid));
                Version = version ?? throw new ArgumentNullException(nameof(version));
            }

            public string Name { get; }
            public string FileName { get; }
            /// <summary>
            /// 使用 <see langword="string"/>，易语言内部是按照字符串的大小写不敏感方式比较的
            /// </summary>
            public string Guid { get; }
            public Version Version { get; }
        }
        public class ECom : DependencyModel
        {
            public ECom(string name, string path, bool reExport)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Path = path ?? throw new ArgumentNullException(nameof(path));
                ReExport = reExport;
            }

            public string Name { get; }
            public string Path { get; }
            public bool ReExport { get; }
        }
    }
}
