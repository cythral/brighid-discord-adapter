using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Author of a message embed.
    /// </summary>
    public struct EmbedAuthor
    {
        /// <summary>
        /// Gets or sets the name of author.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the url of author.
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the url of author icon (only supports http(s) and attachments).
        /// </summary>
        [JsonPropertyName("icon_url")]
        public string? IconUrl { get; set; }

        /// <summary>
        /// Gets or sets a proxied url of author icon.
        /// </summary>
        [JsonPropertyName("proxy_icon_url")]
        public string? ProxyIconUrl { get; set; }
    }
}
