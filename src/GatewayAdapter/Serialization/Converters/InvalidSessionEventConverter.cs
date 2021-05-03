using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Brighid.Discord.GatewayAdapter.Events;

#pragma warning disable CS1591, SA1600
namespace Brighid.Discord.GatewayAdapter.Serialization
{
    public class InvalidSessionEventConverter : JsonConverter<InvalidSessionEvent>
    {
        public override InvalidSessionEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var isResumable = reader.GetBoolean();
            return new InvalidSessionEvent(isResumable);
        }

        public override void Write(Utf8JsonWriter writer, InvalidSessionEvent value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value.IsResumable);
        }
    }
}
