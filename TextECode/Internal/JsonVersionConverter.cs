using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenEpl.TextECode.Internal
{
    /// <summary>
    /// Used to serialize and deserialize <see cref="Version"/>
    /// </summary>
    internal class JsonVersionConverter : JsonConverter<Version>
    {
        public override Version Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return Version.Parse(reader.GetString());
        }

        public override void Write(
            Utf8JsonWriter writer,
            Version version,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(version.ToString());
        }
    }
}