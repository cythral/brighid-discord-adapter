using System;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS1591, SA1600
namespace Brighid.Discord.Models
{
    public class SnowflakeConverter : JsonConverter<Snowflake>
    {
        public override Snowflake Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();
            ulong longValue = stringValue != null ? ulong.Parse(stringValue) : 0;
            return new Snowflake(longValue);
        }

        public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value.ToString());
        }
    }
}
