using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Management
{
    /// <inheritdoc />
    public class DnsService : IDnsService
    {
        /// <inheritdoc />
        public async Task<IEnumerable<IPAddress>> GetIPAddresses(string host, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Dns.GetHostAddressesAsync(host, AddressFamily.InterNetworkV6, cancellationToken);
        }
    }
}
