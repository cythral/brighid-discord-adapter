using System.Text.Json.Serialization;

using Brighid.Discord.AlarmNotifier;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, GenerationMode = JsonSourceGenerationMode.Default, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(ExecuteWebhookPayload))]
    public partial class JsonContext : JsonSerializerContext
    {
    }
}
