using System;

using Brighid.Discord.Adapter.Users;
using Brighid.Identity.Client;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

#pragma warning disable IDE0060

namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// Controls internal caches.
    /// </summary>
    [Route("/cache")]
    [Authorize(Roles = "CacheManager")]
    public class CacheController : Controller
    {
        private readonly IUserIdCache userIdCache;
        private readonly ITokenStore tokenStore;
        private readonly IUserTokenStore userTokenStore;
        private readonly ILogger<CacheController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheController" /> class.
        /// </summary>
        /// <param name="userIdCache">Cache to control user IDs.</param>
        /// <param name="tokenStore">Store for Identity Bearer Tokens.</param>
        /// <param name="userTokenStore">Store for Identity User/Impersonate Bearer Tokens.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public CacheController(
            IUserIdCache userIdCache,
            ITokenStore tokenStore,
            IUserTokenStore userTokenStore,
            ILogger<CacheController> logger
        )
        {
            this.userIdCache = userIdCache;
            this.tokenStore = tokenStore;
            this.userTokenStore = userTokenStore;
            this.logger = logger;
        }

        /// <summary>
        /// Clears all caches related to users.
        /// </summary>
        /// <returns>OK if successful.</returns>
        [HttpDelete]
        public StatusCodeResult ClearAll()
        {
            userIdCache.Clear();
            tokenStore.InvalidateToken();
            userTokenStore.InvalidateAllUserTokens();
            logger.LogInformation("Cleared all internal caches.");

            return Ok();
        }

        /// <summary>
        /// Clear all caches related to the user identified by the given <paramref name="identityId" />.
        /// </summary>
        /// <param name="identityId">The Brighid Identity ID of the user to clear cache for.</param>
        /// <returns>OK if successful.</returns>
        [HttpDelete("users/{identityId:guid}")]
        public StatusCodeResult ClearUserSpecificCache(Guid identityId)
        {
            userIdCache.RemoveByIdentityId(identityId);
            userTokenStore.InvalidateTokensForUser(identityId.ToString());
            logger.LogInformation("Cleared user-specific cache for: {@id}", identityId);

            return Ok();
        }
    }
}
