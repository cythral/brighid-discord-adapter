using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A field on a message embed.
    /// </summary>
    public struct EmbedField
    {
        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value of the field.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this field should display inline.
        /// </summary>
        [JsonPropertyName("inline")]
        public bool? Inline { get; set; }
    }
}
