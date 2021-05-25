using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Networking
{
    /// <inheritdoc />
    public class DefaultTcpClient : ITcpClient
    {
        private readonly TcpClient innerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTcpClient" /> class.
        /// </summary>
        /// <param name="innerClient">The inner TCP Client.</param>
        public DefaultTcpClient(
            TcpClient innerClient
        )
        {
            this.innerClient = innerClient;
        }

        /// <inheritdoc />
        public Stream GetStream()
        {
            return innerClient.GetStream();
        }

        /// <inheritdoc />
        public async Task Write(string payload, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var writer = new StreamWriter(GetStream());
            await writer.WriteAsync(payload.ToCharArray(), cancellationToken);
            await writer.FlushAsync();
        }

        /// <inheritdoc />
        public void Close()
        {
            innerClient.Close();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            innerClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
