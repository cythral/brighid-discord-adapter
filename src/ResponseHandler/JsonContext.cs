using System.Text.Json.Serialization;

using Brighid.Discord.Models.Payloads;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, GenerationMode = JsonSourceGenerationMode.Default, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(Models.Request))]
    [JsonSerializable(typeof(Models.EmbedType))]
    [JsonSerializable(typeof(CreateMessagePayload))]
    public partial class JsonContext : JsonSerializerContext
    {
    }
}
