using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.CacheExpirer
{
    public interface IDnsService
    {
        Task<IEnumerable<IPAddress>> GetIPAddresses(string host, CancellationToken cancellationToken);
    }
}
