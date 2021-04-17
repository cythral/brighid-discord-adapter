using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Footer of a message embed.
    /// </summary>
    public struct EmbedFooter
    {
        /// <summary>
        /// Gets or sets the footer text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the url of footer icon (only supports http(s) and attachments).
        /// </summary>
        [JsonPropertyName("icon_url")]
        public string? IconUrl { get; set; }

        /// <summary>
        /// Gets or sets a proxied url of footer icon.
        /// </summary>
        [JsonPropertyName("proxy_icon_url")]
        public string? ProxyIconUrl { get; set; }
    }
}
