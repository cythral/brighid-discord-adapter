using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Users
{
    /// <summary>
    /// Service for managing users.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Exchanges a user's OAuth2 code for a token, so that we can call the Discord API on behalf of the user.
        /// </summary>
        /// <param name="code">Code to exchange for a token.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting token.</returns>
        Task<string> ExchangeOAuth2CodeForToken(string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve's a user's Discord ID using an OAuth Token.
        /// </summary>
        /// <param name="token">Token to use when making a request to retrieve the Discord ID.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The user's ID.</returns>
        Task<User> GetDiscordUserInfo(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Link a Discord ID to the current user, given their Brighid Identity Access Token.
        /// </summary>
        /// <param name="discordUserId">The Discord ID to link to the user.</param>
        /// <param name="identityUserId">The ID of the user on Brighid Identity.</param>
        /// <param name="accessToken">The user's Brighid Identity Access Token.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task LinkDiscordIdToUser(Snowflake discordUserId, Guid identityUserId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads an Identity Token JWT and returns the User ID from it.
        /// </summary>
        /// <param name="identityToken">The identity token to read.</param>
        /// <returns>The user ID from the token.</returns>
        Guid GetUserIdFromIdentityToken(string identityToken);

        /// <summary>
        /// Determines if a user is registered or not.
        /// </summary>
        /// <param name="user">The user to check registration for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>True if the user is registered, or false if not.</returns>
        Task<bool> IsUserRegistered(User user, CancellationToken cancellationToken = default);
    }
}
