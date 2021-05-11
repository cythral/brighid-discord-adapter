using System.Threading;
using System.Threading.Tasks;

using Brighid.Identity.Client;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Users
{
    /// <inheritdoc />
    public class DefaultUserService : IUserService
    {
        private readonly IUserIdCache userIdCache;
        private readonly ILoginProvidersClient loginProvidersClient;
        private readonly ILogger<DefaultUserService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUserService" /> class.
        /// </summary>
        /// <param name="userIdCache">Cache for user ID lookups.</param>
        /// <param name="loginProvidersClient">Client used to manage login providers and linked users in Brighid Identity.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultUserService(
            IUserIdCache userIdCache,
            ILoginProvidersClient loginProvidersClient,
            ILogger<DefaultUserService> logger
        )
        {
            this.userIdCache = userIdCache;
            this.loginProvidersClient = loginProvidersClient;
            this.logger = logger;
        }

#pragma warning disable IDE0046

        /// <inheritdoc />
        public async Task<bool> IsUserRegistered(Models.User user, CancellationToken cancellationToken)
        {
            if (userIdCache.ContainsKey(user.Id))
            {
                logger.LogInformation("Given userId:{@id} found in cache", user.Id);
                return true;
            }

            try
            {
                var result = await loginProvidersClient.GetUserByLoginProviderKey("discord", user.Id, cancellationToken);
                userIdCache.Add(user.Id, result.Id);
                return true;
            }
            catch (ApiException exception)
            {
                logger.LogError("Received exception from Brighid Identity: {@exception}", exception);
                return false;
            }
        }
    }
}
