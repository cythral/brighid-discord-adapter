using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Brighid.Discord.GatewayAdapter.Messages;

namespace Brighid.Discord.Mocks
{
    public class MockGatewayMessageConverter : JsonConverter<GatewayMessage>
    {
        public override GatewayMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return default;
        }

        public override void Write(Utf8JsonWriter writer, GatewayMessage value, JsonSerializerOptions options)
        {
        }
    }
}
