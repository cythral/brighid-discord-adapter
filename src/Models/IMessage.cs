using System;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A message that is sent by a user or bot to Discord.
    /// </summary>
    [JsonConverter(typeof(IMessageConverter))]
    public interface IMessage
    {
        /// <summary>
        /// Gets or sets id of the message.
        /// </summary>
        [JsonPropertyName("id")]
        Snowflake Id { get; set; }

        /// <summary>
        /// Gets or sets id of the channel the message was sent in.
        /// </summary>
        [JsonPropertyName("channel_id")]
        Snowflake ChannelId { get; set; }

        /// <summary>
        /// Gets or sets id of the guild the message was sent in.
        /// </summary>
        [JsonPropertyName("guild_id")]
        Snowflake? GuildId { get; set; }

        /// <summary>
        /// Gets or sets the author of this message.
        /// </summary>
        [JsonPropertyName("author")]
        User Author { get; set; }

        /// <summary>
        /// Gets or sets member properties for this message's author.
        /// </summary>
        [JsonPropertyName("member")]
        GuildMember? Member { get; set; }

        /// <summary>
        /// Gets or sets contents of the message.
        /// </summary>
        [JsonPropertyName("content")]
        string Content { get; set; }

        /// <summary>
        /// Gets or sets when this message was sent.
        /// </summary>
        [JsonPropertyName("timestamp")]
        DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets when this message was edited (or null if never).
        /// </summary>
        [JsonPropertyName("edited_timestamp")]
        DateTime? EditedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this was a TTS message.
        /// </summary>
        [JsonPropertyName("tts")]
        bool IsTextToSpeechMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this message mentions everyone.
        /// </summary>
        [JsonPropertyName("mention_everyone")]
        bool MentionsEveryone { get; set; }

        /// <summary>
        /// Gets or sets array of user objects, with an additional partial member field users specifically mentioned in the message.
        /// </summary>
        [JsonPropertyName("mentions")]
        UserMention[] Mentions { get; set; }

        /// <summary>
        /// Gets or sets roles specifically mentioned in this message.
        /// </summary>
        [JsonPropertyName("mention_roles")]
        Snowflake[] MentionedRoles { get; set; }

        /// <summary>
        /// Gets or sets array of channel mention objects channels specifically mentioned in this message.
        /// </summary>
        [JsonPropertyName("mention_channels")]
        ChannelMention[]? MentionedChannels { get; set; }

        /// <summary>
        /// Gets or sets any attached files.
        /// </summary>
        [JsonPropertyName("attachments")]
        Attachment[] Attachments { get; set; }

        /// <summary>
        /// Gets or sets any embedded content.
        /// </summary>
        [JsonPropertyName("embeds")]
        Embed[] Embeds { get; set; }

        /// <summary>
        /// Gets or sets reactions to the message.
        /// </summary>
        [JsonPropertyName("reactions")]
        Reaction[] Reactions { get; set; }

        /// <summary>
        /// Gets or sets a value used for validating a message was sent.
        /// </summary>
        [JsonPropertyName("nonce")]
        string? Nonce { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this message is pinned.
        /// </summary>
        [JsonPropertyName("pinned")]
        bool IsPinned { get; set; }

        /// <summary>
        /// Gets or sets a value that if the message is generated by a webhook, is the webhook's id.
        /// </summary>
        [JsonPropertyName("webhook_id")]
        Snowflake? WebhookId { get; set; }

        /// <summary>
        /// Gets or sets the type of message.
        /// </summary>
        [JsonPropertyName("type")]
        MessageType Type { get; set; }

        /// <summary>
        /// Gets or sets the message activity object sent with Rich Presence-related chat embeds.
        /// </summary>
        [JsonPropertyName("activity")]
        Activity? Activity { get; set; }

        /// <summary>
        /// Gets or sets the application object sent with Rich Presence-related chat embeds.
        /// </summary>
        [JsonPropertyName("application")]
        Application? Application { get; set; }

        /// <summary>
        /// Gets or sets data showing the source of a crosspost, channel follow add, pin, or reply message.
        /// </summary>
        [JsonPropertyName("message_reference")]
        MessageReference? MessageReference { get; set; }

        /// <summary>
        /// Gets or sets message flags combined as a bitfield.
        /// </summary>
        [JsonPropertyName("flags")]
        MessageFlags? Flags { get; set; }

        /// <summary>
        /// Gets or sets the stickers sent with the message(bots currently can only receive messages with stickers, not send).
        /// </summary>
        [JsonPropertyName("stickers")]
        Sticker[]? Stickers { get; set; }

        /// <summary>
        /// Gets or sets the message associated with the Message Reference.
        /// </summary>
        [JsonPropertyName("referenced_message")]
        IMessage ReferencedMessage { get; set; }

        /// <summary>
        /// Gets or sets message interaction object sent if the message is a response to an Interaction.
        /// </summary>
        [JsonPropertyName("interaction")]
        Interaction? Interaction { get; set; }
    }
}
