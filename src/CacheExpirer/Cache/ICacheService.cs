using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.CacheExpirer
{
    public interface ICacheService
    {
        Task ExpireUserCache(IPAddress ip, string userId, CancellationToken cancellationToken);

        Task ExpireCommandCache(IPAddress ip, string cacheId, CancellationToken cancellationToken);
    }
}
