using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Discord group / guild.
    /// </summary>
    public class Guild
    {
        /// <summary>
        /// Gets or sets the guild id.
        /// </summary>
        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this guild is unavailable due to an outage.
        /// </summary>
        [JsonPropertyName("unavailable")]
        public bool IsUnavailable { get; set; }
    }
}
