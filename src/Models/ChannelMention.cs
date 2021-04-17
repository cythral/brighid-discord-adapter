using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A mentioned channel.
    /// </summary>
    public struct ChannelMention
    {
        /// <summary>
        /// Gets or sets the id of the channel.
        /// </summary>
        [JsonPropertyName("id")]
        public Snowflake Id { get; set; }

        /// <summary>
        /// Gets or sets id of the guild containing the channel.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Snowflake GuildId { get; set; }

        /// <summary>
        /// Gets or sets the type of channel.
        /// </summary>
        [JsonPropertyName("type")]
        public ChannelType Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
