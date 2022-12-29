using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using VersionConverter = Newtonsoft.Json.Converters.VersionConverter;

namespace OpenEpl.TextECode.Model
{
    public enum EplLanguage
    {
        GBK = 1,
        ASCII,
        BIG5,
        SJIS
    }
    public enum EplProjectType
    {
        WindowsApp = 0,
        WindowsConsole = 1,
        WindowsLibrary = 2,
        WindowsECom = 1000,
        LinuxConsole = 10000,
        LinuxECom = 11000,
    }
    public class ProjectModel
    {
        public string Name { get; set; }

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EplProjectType ProjectType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(EplLanguage.GBK)]
        public EplLanguage Language { get; set; } = EplLanguage.GBK;

        public string SourceSet { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OutFile { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Icon { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Author { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ZipCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TelephoneNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FaxNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Homepage { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Copyright { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(true)]
        public bool WriteVersion { get; set; } = true;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CompilePlugins { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool ExportPublicClassMethod { get; set; }

        public List<DependencyModel> Dependencies { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }
}
