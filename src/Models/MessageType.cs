namespace Brighid.Discord.Models
{
    /// <summary>
    /// Type of message.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Default message type.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Add recipient message type.
        /// </summary>
        RecipientAdd = 1,

        /// <summary>
        /// Remove recipient message type.
        /// </summary>
        RecipientRemove = 2,

        /// <summary>
        /// Call message type.
        /// </summary>
        Call = 3,

        /// <summary>
        /// Channel name changed message type.
        /// </summary>
        ChannelNameChange = 4,

        /// <summary>
        /// Channel icon changed message type.
        /// </summary>
        ChannelIconChange = 5,

        /// <summary>
        /// Channel pinned message type.
        /// </summary>
        ChannelPinnedMessage = 6,

        /// <summary>
        /// Guild member joined message type.
        /// </summary>
        GuildMemberJoin = 7,

        /// <summary>
        /// User premium guild subscription message type.
        /// </summary>
        UserPremiumGuildSubscription = 8,

        /// <summary>
        /// User premium guild subscription tier 1 message type.
        /// </summary>
        UserPremiumGuildSubscriptionTier1 = 9,

        /// <summary>
        /// User premium guild subscription tier 2 message type.
        /// </summary>
        UserPremiumGuildSubscriptionTier2 = 10,

        /// <summary>
        /// User premium guild subscription tier 3 message type.
        /// </summary>
        UserPremiumGuildSubscriptionTier3 = 11,

        /// <summary>
        /// Channel follow added message type.
        /// </summary>
        ChannelFollowAdd = 12,

        /// <summary>
        /// Guild discovery disqualified message type.
        /// </summary>
        GuildDiscoveryDisqualified = 14,

        /// <summary>
        /// Guild discovery requalified message type.
        /// </summary>
        GuildDiscoveryRequalified = 15,

        /// <summary>
        /// Guild discovery grace period initial warning message type.
        /// </summary>
        GuildDiscoveryGracePeriodInitialWarning = 16,

        /// <summary>
        /// Guild discovery grace period final warning message type.
        /// </summary>
        GuildDiscoveryGracePeriodFinalWarning = 17,

        /// <summary>
        /// Reply message type.
        /// </summary>
        Reply = 19,

        /// <summary>
        /// Application command message type.
        /// </summary>
        ApplicationCommand = 20,

        /// <summary>
        /// Guild invite reminder message type.
        /// </summary>
        GuildInviteReminder = 22,
    }
}
