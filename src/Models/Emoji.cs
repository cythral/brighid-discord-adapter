using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// An emoji.
    /// </summary>
    public struct Emoji
    {
        /// <summary>
        /// Gets or sets the emoji id.
        /// </summary>
        [JsonPropertyName("id")]
        public Snowflake? Id { get; set; }

        /// <summary>
        /// Gets or sets the emoji name.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the roles allowed to use this emoji.
        /// </summary>
        [JsonPropertyName("roles")]
        public Snowflake[]? Roles { get; set; }

        /// <summary>
        /// Gets or sets the user that created this emoji.
        /// </summary>
        [JsonPropertyName("user")]
        public User? User { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this emoji must be wrapped in colons.
        /// </summary>
        [JsonPropertyName("require_colons")]
        public bool? RequiresColons { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this emoji is managed.
        /// </summary>
        [JsonPropertyName("managed")]
        public bool? IsManaged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this emoji is animated.
        /// </summary>
        [JsonPropertyName("animated")]
        public bool? IsAnimated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this emoji can be used, may be false due to loss of Server Boosts.
        /// </summary>
        [JsonPropertyName("available")]
        public bool? IsAvailable { get; set; }
    }
}
