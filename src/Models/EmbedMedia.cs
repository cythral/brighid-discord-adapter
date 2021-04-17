using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A media object in a message embed.
    /// </summary>
    public struct EmbedMedia
    {
        /// <summary>
        /// Gets or sets source url of media (only supports http(s) and attachments).
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets a proxied url of the media.
        /// </summary>
        [JsonPropertyName("proxy_url")]
        public string? ProxyUrl { get; set; }

        /// <summary>
        /// Gets or sets the height of the media.
        /// </summary>
        [JsonPropertyName("height")]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the width of the media.
        /// </summary>
        [JsonPropertyName("width")]
        public int? Width { get; set; }
    }
}
