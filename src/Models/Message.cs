using System;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <inheritdoc />
    public struct Message : IMessage
    {
        /// <inheritdoc />
        [JsonPropertyName("id")]
        public Snowflake Id { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("channel_id")]
        public Snowflake ChannelId { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("guild_id")]
        public Snowflake? GuildId { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("author")]
        public User Author { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("member")]
        public GuildMember? Member { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("content")]
        public string Content { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("edited_timestamp")]
        public DateTime? EditedTimestamp { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("tts")]
        public bool IsTextToSpeechMessage { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("mention_everyone")]
        public bool MentionsEveryone { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("mentions")]
        public UserMention[] Mentions { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("mention_roles")]
        public Snowflake[] MentionedRoles { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("mention_channels")]
        public ChannelMention[]? MentionedChannels { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("attachments")]
        public Attachment[] Attachments { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("embeds")]
        public Embed[] Embeds { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("reactions")]
        public Reaction[] Reactions { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("nonce")]
        public string? Nonce { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("pinned")]
        public bool IsPinned { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("webhook_id")]
        public Snowflake? WebhookId { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("type")]
        public MessageType Type { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("activity")]
        public Activity? Activity { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("application")]
        public Application? Application { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("message_reference")]
        public MessageReference? MessageReference { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("flags")]
        public MessageFlags? Flags { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("stickers")]
        public Sticker[]? Stickers { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("referenced_message")]
        public IMessage ReferencedMessage { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("interaction")]
        public Interaction? Interaction { get; set; }
    }
}
