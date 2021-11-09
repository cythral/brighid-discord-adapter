using System.Diagnostics.CodeAnalysis;

namespace Brighid.Discord.Adapter
{
    /// <summary>
    /// View Model for the Account Link Success Page.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class AccountLinkViewModel
    {
        /// <summary>
        /// Gets or sets the Account Link Status.
        /// </summary>
        public AccountLinkStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the Discord User ID.
        /// </summary>
        public string? DiscordUserId { get; set; }

        /// <summary>
        /// Gets or sets the Discord Avatar Hash.
        /// </summary>
        public string? DiscordAvatarHash { get; set; }

        /// <summary>
        /// Gets or sets the Discord Username.
        /// </summary>
        public string? DiscordUsername { get; set; }

        /// <summary>
        /// Gets or sets the Discord Discriminator.
        /// </summary>
        public string? DiscordDiscriminator { get; set; }
    }
}
