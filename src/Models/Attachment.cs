using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A message attachment.
    /// </summary>
    public struct Attachment
    {
        /// <summary>
        /// Gets or sets the attachment id.
        /// </summary>
        [JsonPropertyName("id")]
        public Snowflake Id { get; set; }

        /// <summary>
        /// Gets or sets the name of file attached.
        /// </summary>
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the attachment's media type.
        /// </summary>
        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the size of file in bytes.
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the source url of file.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a proxied url of file.
        /// </summary>
        [JsonPropertyName("proxy_url")]
        public string ProxyUrl { get; set; }

        /// <summary>
        /// Gets or sets height of file (if image).
        /// </summary>
        [JsonPropertyName("height")]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets width of file (if image).
        /// </summary>
        [JsonPropertyName("width")]
        public int? Width { get; set; }
    }
}
