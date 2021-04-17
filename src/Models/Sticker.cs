using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A message sticker.
    /// </summary>
    public struct Sticker
    {
        /// <summary>
        /// Gets or sets id of the sticker.
        /// </summary>
        [JsonPropertyName("id")]
        public Snowflake Id { get; set; }

        /// <summary>
        /// Gets or sets id of the pack the sticker is from.
        /// </summary>
        [JsonPropertyName("pack_id")]
        public Snowflake PackId { get; set; }

        /// <summary>
        /// Gets or sets name of the sticker.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets description of the sticker.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of tags for the sticker.
        /// </summary>
        [JsonPropertyName("tags")]
        public string[]? Tags { get; set; }

        /// <summary>
        /// Gets or sets sticker asset hash.
        /// </summary>
        [JsonPropertyName("asset")]
        public string Asset { get; set; }

        /// <summary>
        /// Gets or sets sticker preview asset hash.
        /// </summary>
        [JsonPropertyName("preview_asset")]
        public string PreviewAsset { get; set; }

        /// <summary>
        /// Gets or sets type of sticker format.
        /// </summary>
        [JsonPropertyName("format_type")]
        public StickerType FormatType { get; set; }
    }
}
