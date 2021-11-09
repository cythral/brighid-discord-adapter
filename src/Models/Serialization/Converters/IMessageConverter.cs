using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

#pragma warning disable CS1591, SA1600
namespace Brighid.Discord.Models
{
    public class IMessageConverter : JsonConverter<IMessage>
    {
        public override IMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var typeInfo = GetJsonTypeInfo(options);
            return JsonSerializer.Deserialize(ref reader, typeInfo);
        }

        public override void Write(Utf8JsonWriter writer, IMessage value, JsonSerializerOptions options)
        {
            var typeInfo = GetJsonTypeInfo(options);
            JsonSerializer.Serialize(writer, (Message)value, typeInfo);
        }

        private static JsonTypeInfo<Message> GetJsonTypeInfo(JsonSerializerOptions options)
        {
            var query = from converter in options.Converters
                        where converter is IServiceProvider
                        select (IServiceProvider)converter;

            var provider = query.FirstOrDefault() ?? throw new Exception("Could not find service provider.");
            var context = provider.GetService(typeof(JsonSerializerContext)) as JsonSerializerContext ?? throw new Exception("Could not find JsonSerializerContext");
            return context.GetTypeInfo(typeof(Message)) as JsonTypeInfo<Message> ?? throw new Exception("Could not find type info for Message");
        }
    }
}
