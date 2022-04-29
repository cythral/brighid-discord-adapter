using System;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A message embed.
    /// </summary>
    public struct Embed
    {
        /// <summary>
        /// Gets or sets the title of embed.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the type of embed (always "rich" for webhook embeds).
        /// </summary>
        [JsonPropertyName("type")]
        public EmbedType? Type { get; set; }

        /// <summary>
        /// Gets or sets the description of embed.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the url of embed.
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of embed content.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the color code of the embed.
        /// </summary>
        [JsonPropertyName("color")]
        public int? Color { get; set; }

        /// <summary>
        /// Gets or sets the footer information.
        /// </summary>
        [JsonPropertyName("footer")]
        public EmbedFooter? Footer { get; set; }

        /// <summary>
        /// Gets or sets the image information.
        /// </summary>
        [JsonPropertyName("image")]
        public EmbedMedia? Image { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail information.
        /// </summary>
        [JsonPropertyName("thumbnail")]
        public EmbedMedia? Thumbnail { get; set; }

        /// <summary>
        /// Gets or sets the video information.
        /// </summary>
        [JsonPropertyName("video")]
        public EmbedMedia? Video { get; set; }

        /// <summary>
        /// Gets or sets the provider information.
        /// </summary>
        [JsonPropertyName("provider")]
        public EmbedProvider? Provider { get; set; }

        /// <summary>
        /// Gets or sets the author information.
        /// </summary>
        [JsonPropertyName("author")]
        public EmbedAuthor? Author { get; set; }

        /// <summary>
        /// Gets or sets the fields information.
        /// </summary>
        [JsonPropertyName("fields")]
        public EmbedField[] Fields { get; set; }
    }
}
