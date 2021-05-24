using System.IO;
using System.Net.Sockets;

namespace Brighid.Discord.RestClient.Responses
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
    }
}
