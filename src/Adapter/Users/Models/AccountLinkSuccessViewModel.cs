namespace Brighid.Discord.Adapter
{
    /// <summary>
    /// View Model for the Account Link Success Page.
    /// </summary>
    public class AccountLinkSuccessViewModel
    {
        /// <summary>
        /// Gets or sets the Discord User ID.
        /// </summary>
        public string DiscordUserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Discord Avatar Hash.
        /// </summary>
        public string DiscordAvatarHash { get; set; } = string.Empty;
    }
}
