using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.CacheExpirer
{
    public class DnsService : IDnsService
    {
        public async Task<IEnumerable<IPAddress>> GetIPAddresses(Uri host, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Dns.GetHostAddressesAsync(host.ToString(), AddressFamily.Unspecified, cancellationToken);
        }
    }
}