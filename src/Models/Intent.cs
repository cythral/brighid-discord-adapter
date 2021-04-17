using System;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A Gateway Intent.
    /// </summary>
    [Flags]
    public enum Intent
    {
        /// <summary>
        /// The GUILDS intent.
        /// </summary>
        Guilds = 1 << 0,

        /// <summary>
        /// The GUILD_MEMBERS intent.
        /// </summary>
        GuildMembers = 1 << 1,

        /// <summary>
        /// The GUILD_BANS intent.
        /// </summary>
        GuildBans = 1 << 2,

        /// <summary>
        /// The GUILD_EMOJIS intent.
        /// </summary>
        GuildEmojis = 1 << 3,

        /// <summary>
        /// The GUILD_INTEGRATIONS intent.
        /// </summary>
        GuildIntegrations = 1 << 4,

        /// <summary>
        /// The GUILD_WEBHOOKS intent.
        /// </summary>
        GuildWebhooks = 1 << 5,

        /// <summary>
        /// The GUILD_INVITES intent.
        /// </summary>
        GuildInvites = 1 << 6,

        /// <summary>
        /// The GUILD_VOICE_STATES intent.
        /// </summary>
        GuildVoiceStates = 1 << 7,

        /// <summary>
        /// The GUILD_PRESENCES intent.
        /// </summary>
        GuildPresences = 1 << 8,

        /// <summary>
        /// The GUILD_MESSAGES intent.
        /// </summary>
        GuildMessages = 1 << 9,

        /// <summary>
        /// The GUILD_MESSAGE_REACTIONS intent.
        /// </summary>
        GuildMessageReactions = 1 << 10,

        /// <summary>
        /// The GUILD_MESSAGE_TYPING intent.
        /// </summary>
        GuildMessageTyping = 1 << 11,

        /// <summary>
        /// The DIRECT_MESSAGES intent.
        /// </summary>
        DirectMessages = 1 << 12,

        /// <summary>
        /// The DIRECT_MESSAGE_REACTIONS intent.
        /// </summary>
        DirectMessageReactions = 1 << 13,

        /// <summary>
        /// The DIRECT_MESSAGE_TYPING intent.
        /// </summary>
        DirectMessageTyping = 1 << 14,
    }
}
