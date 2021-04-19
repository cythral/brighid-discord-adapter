using System.Text.Json.Serialization;

using Brighid.Discord.Models;

using Destructurama.Attributed;

namespace Brighid.Discord.Events
{
    /// <summary>
    /// Used to trigger the initial handshake with the gateway.
    /// </summary>
    [GatewayEvent(GatewayOpCode.Identify)]
    public struct IdentifyEvent : IGatewayEvent
    {
        /// <summary>
        /// Gets or sets the authentication token.
        /// </summary>
        [JsonPropertyName("token")]
        [NotLogged]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the connection properties.
        /// </summary>
        [JsonPropertyName("properties")]
        public ConnectionProperties ConnectionProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this connection supports compression of packets.
        /// </summary>
        [JsonPropertyName("compress")]
        public bool IsCompressionSupported { get; set; }

        /// <summary>
        /// Gets or sets a value between 50 and 250, total number of members where the gateway will stop sending offline members in the guild member list.
        /// </summary>
        [JsonPropertyName("large_threshold")]
        public int LargeGuildThreshold { get; set; }

        /// <summary>
        /// Gets or sets array of two integers (shard_id, num_shards) used for Guild Sharding.
        /// </summary>
        [JsonPropertyName("shard")]
        public int[] Shard { get; set; }

        /// <summary>
        /// Gets or sets presence structure for initial presence information.
        /// </summary>
        [JsonPropertyName("presence")]
        public PresenceUpdate? Presence { get; set; }

        /// <summary>
        /// Gets or sets the Gateway Intents you wish to receive.
        /// </summary>
        [JsonPropertyName("intents")]
        public Intent Intents { get; set; }
    }
}
