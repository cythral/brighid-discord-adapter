using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Networking
{
    /// <inheritdoc />
    public class DefaultTcpClientFactory : ITcpClientFactory
    {
        /// <inheritdoc />
        public async Task<ITcpClient> CreateTcpClient(string host, int port, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var innerClient = new TcpClient();
            await innerClient.ConnectAsync(host, port, cancellationToken);
            return new DefaultTcpClient(innerClient);
        }
    }
}
