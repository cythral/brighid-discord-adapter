using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.CacheExpirer
{
    public class DnsService : IDnsService
    {
        public async Task<IEnumerable<IPAddress>> GetIPAddresses(string host, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Dns.GetHostAddressesAsync(host, AddressFamily.Unspecified, cancellationToken);
        }
    }
}
