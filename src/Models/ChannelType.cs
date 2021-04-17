namespace Brighid.Discord.Models
{
    /// <summary>
    /// A type of channel on Discord.
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// A text channel within a server.
        /// </summary>
        GuildText = 0,

        /// <summary>
        /// A direct message between users.
        /// </summary>
        DirectMessage = 1,

        /// <summary>
        /// A voice channel within a server.
        /// </summary>
        GuildVoice = 2,

        /// <summary>
        /// A direct message between multiple users.
        /// </summary>
        GroupDirectMessage = 3,

        /// <summary>
        /// An organizational category that contains up to 50 channels.
        /// </summary>
        GuildCategory = 4,

        /// <summary>
        /// A channel that users can follow and crosspost into their own server.
        /// </summary>
        GuildNews = 5,

        /// <summary>
        /// A channel in which game developers can sell their game on Discord.
        /// </summary>
        GuildStore = 6,

        /// <summary>
        /// A voice channel for hosting events with an audience.
        /// </summary>
        GuildStageVoice = 13,
    }
}
