using System;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable CS1591, SA1600
namespace Brighid.Discord.Models
{
    public class IMessageConverter : JsonConverter<IMessage>
    {
        public override IMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Message>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, IMessage value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
