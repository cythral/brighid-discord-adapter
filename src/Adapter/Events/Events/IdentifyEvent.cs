using System.Text.Json.Serialization;

using Brighid.Discord.Models;

using Destructurama.Attributed;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Used to trigger the initial handshake with the gateway.
    /// </summary>
    [GatewayEvent(GatewayOpCode.Identify)]
    public readonly struct IdentifyEvent : IGatewayEvent
    {
        /// <summary>
        /// Gets the authentication token.
        /// </summary>
        [JsonPropertyName("token")]
        [NotLogged]
        public string Token { get; init; }

        /// <summary>
        /// Gets the connection properties.
        /// </summary>
        [JsonPropertyName("properties")]
        public ConnectionProperties ConnectionProperties { get; init; }

        /// <summary>
        /// Gets a value indicating whether this connection supports compression of packets.
        /// </summary>
        [JsonPropertyName("compress")]
        public bool IsCompressionSupported { get; init; }

        /// <summary>
        /// Gets a value between 50 and 250, total number of members where the gateway will stop sending offline members in the guild member list.
        /// </summary>
        [JsonPropertyName("large_threshold")]
        public int LargeGuildThreshold { get; init; }

        /// <summary>
        /// Gets array of two integers (shard_id, num_shards) used for Guild Sharding.
        /// </summary>
        [JsonPropertyName("shard")]
        public int[] Shard { get; init; }

        /// <summary>
        /// Gets presence structure for initial presence information.
        /// </summary>
        [JsonPropertyName("presence")]
        public PresenceUpdate? Presence { get; init; }

        /// <summary>
        /// Gets the Gateway Intents you wish to receive.
        /// </summary>
        [JsonPropertyName("intents")]
        public Intent Intents { get; init; }
    }
}
