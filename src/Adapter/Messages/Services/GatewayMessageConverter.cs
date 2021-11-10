using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;

#pragma warning disable CS1591, SA1600
namespace Brighid.Discord.Adapter.Messages
{
    public class GatewayMessageConverter : JsonConverter<GatewayMessage>
    {
        private readonly IEventDataConverter eventDataConverter;

        public GatewayMessageConverter()
            : this(new GeneratedEventDataConverter())
        {
        }

        internal GatewayMessageConverter(IEventDataConverter eventDataConverter)
        {
            this.eventDataConverter = eventDataConverter;
        }

        public override GatewayMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var messageWithoutData = SystemTextJsonSerializer.Deserialize(ref reader, JsonContext.Default.GatewayMessageWithoutData)!;
            return eventDataConverter.ParseEventData(messageWithoutData);
        }

        public override void Write(Utf8JsonWriter writer, GatewayMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("op", (int)value.OpCode);

            if (value.SequenceNumber != null)
            {
                writer.WriteNumber("s", (int)value.SequenceNumber);
            }

            if (value.EventName != null)
            {
                writer.WriteString("t", value.EventName);
            }

            if (value.Data != null)
            {
                eventDataConverter.WriteEventData(writer, value.Data);
            }

            writer.WriteEndObject();
        }

        private void WriteData<TValue>(Utf8JsonWriter writer, string name, in TValue value, JsonTypeInfo typeInfo)
        {
            Console.WriteLine(typeof(TValue).Name);
            if (value != null && typeInfo is JsonTypeInfo<TValue> jsonTypeInfo && jsonTypeInfo.SerializeHandler != null)
            {
                writer.WritePropertyName(name);
                jsonTypeInfo.SerializeHandler(writer, value);
                writer.Flush();
                return;
            }
        }
    }
}
