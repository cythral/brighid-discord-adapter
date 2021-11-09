using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;

#pragma warning disable CS1591, SA1600
namespace Brighid.Discord.Adapter.Events
{
    public class IMessageEventConverter<TMessageEvent> : JsonConverter<TMessageEvent>
        where TMessageEvent : IMessageEvent, new()
    {
        public override TMessageEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new TMessageEvent()
            {
                Message = SystemTextJsonSerializer.Deserialize(ref reader, JsonContext.Default.Message),
            };
        }

        public override void Write(Utf8JsonWriter writer, TMessageEvent value, JsonSerializerOptions options)
        {
            SystemTextJsonSerializer.Serialize(writer, value.Message, JsonContext.Default.Message);
        }
    }
}
