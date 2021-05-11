using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;

#pragma warning disable CS1591, SA1600
namespace Brighid.Discord.Adapter.Messages
{
    public class GatewayMessageConverter : JsonConverter<GatewayMessage>
    {
        private readonly IMessageParser parser;
        private readonly IDictionary<string, string> propertyNames;

        public GatewayMessageConverter(IMessageParser parser)
        {
            this.parser = parser;
            propertyNames = (from prop in typeof(GatewayMessage).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             let attributes = prop.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true)
                             let attribute = (JsonPropertyNameAttribute?)attributes.FirstOrDefault()
                             let jsonPropName = attribute == null ? prop.Name : attribute.Name
                             select new { PropertyName = prop.Name, JsonPropertyName = jsonPropName })
                            .ToDictionary(result => result.PropertyName, result => result.JsonPropertyName);
        }

        public override GatewayMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var messageWithoutData = SystemTextJsonSerializer.Deserialize<GatewayMessageWithoutData>(ref reader, options)!;
            return parser.ParseEventData(messageWithoutData, options);
        }

        public override void Write(Utf8JsonWriter writer, GatewayMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            Write(writer, propertyNames["OpCode"], (decimal)(int)value.OpCode, options);
            Write(writer, propertyNames["SequenceNumber"], (decimal?)value.SequenceNumber, options);
            Write(writer, propertyNames["EventName"], value.EventName, options);
            Write(writer, propertyNames["Data"], value.Data, options);
            writer.WriteEndObject();
        }

        private void Write(Utf8JsonWriter writer, string name, object? value, JsonSerializerOptions options)
        {
            if (value == null && (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull || options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault))
            {
                return;
            }

            writer.WritePropertyName(name);
            SystemTextJsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
        }
    }
}
