using System.Text.Json.Serialization;

using Brighid.Commands.Client;
using Brighid.Discord.Adapter.Events;
using Brighid.Discord.Adapter.Management;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Adapter.Users;
using Brighid.Discord.Models.Payloads;

#pragma warning disable CS1591

namespace Brighid.Discord.Adapter
{
    /// <inheritdoc />
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, GenerationMode = JsonSourceGenerationMode.Default, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(Models.User))]
    [JsonSerializable(typeof(Models.Message))]
    [JsonSerializable(typeof(Models.EmbedType))]
    [JsonSerializable(typeof(Models.Request))]
    [JsonSerializable(typeof(Models.Channel))]
    [JsonSerializable(typeof(Models.GatewayBotInfo))]
    [JsonSerializable(typeof(Models.GatewayInfo))]
    [JsonSerializable(typeof(Models.Response))]
    [JsonSerializable(typeof(GatewayMessage))]
    [JsonSerializable(typeof(GatewayMessageWithoutData))]
    [JsonSerializable(typeof(HelloEvent))]
    [JsonSerializable(typeof(ReadyEvent))]
    [JsonSerializable(typeof(MessageCreateEvent))]
    [JsonSerializable(typeof(ReconnectEvent))]
    [JsonSerializable(typeof(HeartbeatEvent))]
    [JsonSerializable(typeof(HeartbeatAckEvent))]
    [JsonSerializable(typeof(IdentifyEvent))]
    [JsonSerializable(typeof(ResumeEvent))]
    [JsonSerializable(typeof(ResumedEvent))]
    [JsonSerializable(typeof(InvalidSessionEvent))]
    [JsonSerializable(typeof(CreateMessagePayload))]
    [JsonSerializable(typeof(OAuth2TokenResponse))]
    [JsonSerializable(typeof(CreateDirectMessageChannelPayload))]
    [JsonSerializable(typeof(IGatewayEvent))]
    [JsonSerializable(typeof(TaskMetadata))]
    [JsonSerializable(typeof(NodeInfo))]
    [JsonSerializable(typeof(CommandsClientOptions))]
    public partial class JsonContext : JsonSerializerContext
    {
    }
}
