using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Reference to an existing message.
    /// </summary>
    public struct MessageReference
    {
        /// <summary>
        /// Gets or sets the id of the originating message.
        /// </summary>
        [JsonPropertyName("message_id")]
        public Snowflake? MessageId { get; set; }

        /// <summary>
        /// Gets or sets the id of the originating message's channel.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public Snowflake? ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the id of the originating message's guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Snowflake? GuildId { get; set; }

        /// <summary>
        /// Gets or sets when sending, whether to error if the referenced message doesn't exist instead of sending as a normal (non-reply) message, default true.
        /// </summary>
        [JsonPropertyName("fail_if_not_exists")]
        public bool? FailIfNotExists { get; set; }
    }
}
