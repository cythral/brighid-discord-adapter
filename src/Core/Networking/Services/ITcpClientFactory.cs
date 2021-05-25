using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Networking
{
    /// <summary>
    /// Factory for creating TCP Clients.
    /// </summary>
    public interface ITcpClientFactory
    {
        /// <summary>
        /// Creates a new TCP Client and connects to the given host and port.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port on the host to connect to.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting TCP Client.</returns>
        Task<ITcpClient> CreateTcpClient(string host, int port, CancellationToken cancellationToken = default);
    }
}
