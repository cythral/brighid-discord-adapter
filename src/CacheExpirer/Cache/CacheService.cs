using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.CacheExpirer
{
    public class CacheService : ICacheService
    {
        private readonly HttpClient httpClient;

        public CacheService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task ExpireUserCache(IPAddress ip, string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var response = await httpClient.DeleteAsync($"http://{ip}/cache/users/{userId}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task ExpireCommandCache(IPAddress ip, string command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var response = await httpClient.DeleteAsync($"http://{ip}/cache/commands/{command}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
