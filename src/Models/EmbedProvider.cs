using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Provider of a message embed.
    /// </summary>
    public struct EmbedProvider
    {
        /// <summary>
        /// Gets or sets name of the provider.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets url of the provider.
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
