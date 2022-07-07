using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// DNS Service.
    /// </summary>
    public interface IDnsService
    {
        /// <summary>
        /// Retrieve all IP addresses for a hostname.
        /// </summary>
        /// <param name="host">The hostname to lookup addresses for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resolved addresses.</returns>
        Task<IEnumerable<IPAddress>> GetIPAddresses(string host, CancellationToken cancellationToken);
    }
}
