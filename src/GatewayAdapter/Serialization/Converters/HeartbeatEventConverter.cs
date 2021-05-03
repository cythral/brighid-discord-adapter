using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Brighid.Discord.GatewayAdapter.Events;

#pragma warning disable CS1591, SA1600
namespace Brighid.Discord.GatewayAdapter.Serialization
{
    public class HeartbeatEventConverter : JsonConverter<HeartbeatEvent>
    {
        public override HeartbeatEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, HeartbeatEvent value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
