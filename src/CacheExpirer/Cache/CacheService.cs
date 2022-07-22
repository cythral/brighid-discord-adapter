using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.CacheExpirer
{
    public class CacheService : ICacheService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<CacheService> logger;

        public CacheService(
            HttpClient httpClient,
            ILogger<CacheService> logger
        )
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task ExpireUserCache(IPAddress ip, string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Expiring cache for user: {@userId} on IP {@ip}", userId, ip);
            using var response = await httpClient.DeleteAsync($"http://[{ip}]/cache/users/{userId}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task ExpireCommandCache(IPAddress ip, string command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Expiring cache for command: {@command} on IP {@ip}", command, ip);
            using var response = await httpClient.DeleteAsync($"http://[{ip}]/cache/commands/{command}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
