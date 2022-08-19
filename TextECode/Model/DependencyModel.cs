using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenEpl.TextECode.Model
{
    public enum DependencyModelKind
    {
        ELib,
        ECom
    }
    [JsonConverter(typeof(JsonSubtypes), "Kind")]
    [JsonSubtypes.KnownSubType(typeof(ELib), DependencyModelKind.ELib)]
    [JsonSubtypes.KnownSubType(typeof(ECom), DependencyModelKind.ECom)]
    public class DependencyModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual DependencyModelKind Kind { get; }
        public class ELib : DependencyModel
        {
            public ELib(string name, string fileName, string guid, Version version)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
                Guid = guid ?? throw new ArgumentNullException(nameof(guid));
                Version = version ?? throw new ArgumentNullException(nameof(version));
            }

            public override DependencyModelKind Kind => DependencyModelKind.ELib;
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

            public override DependencyModelKind Kind => DependencyModelKind.ECom;
            public string Name { get; }
            public string Path { get; }
            public bool ReExport { get; }
        }
    }
}
