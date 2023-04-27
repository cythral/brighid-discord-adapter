using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Represents Information about the Discord Gateway.
    /// </summary>
    public struct GatewayInfo
    {
        /// <summary>
        /// Gets or sets the gateway URL.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
