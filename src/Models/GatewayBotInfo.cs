using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Information about the gateway with additional information pertinent to bots.
    /// </summary>
    public struct GatewayBotInfo
    {
        /// <summary>
        /// Gets or sets the WSS URL that can be used for connecting to the Gateway.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the recommended number of shards to use when connecting.
        /// </summary>
        [JsonPropertyName("shards")]
        public int Shards { get; set; }

        /// <summary>
        /// Gets or sets information on the current session start limit.
        /// </summary>
        [JsonPropertyName("session_start_limit")]
        public SessionStartLimit SessionStartLimit { get; set; }
    }
}
