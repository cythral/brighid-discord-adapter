using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Adapter.Management
{
    /// <inheritdoc />
    public class IpAddressConverter : JsonConverter<IPAddress>
    {
        /// <inheritdoc />
        public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var address = reader.GetString()!;
            return IPAddress.Parse(address);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
