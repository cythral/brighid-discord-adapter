using System.Net.Sockets;
using System.Threading.Tasks;

using Brighid.Discord.Networking;

using Microsoft.Extensions.Options;

namespace Brighid.Discord.RestClient.Responses
{
    /// <inheritdoc />
    public class DefaultTcpListener : ITcpListener
    {
        private readonly TcpListener listener;
        private readonly TcpListenerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTcpListener" /> class.
        /// </summary>
        /// <param name="options">Options to use for the Tcp Listener.</param>
        public DefaultTcpListener(
            IOptions<TcpListenerOptions> options
        )
        {
            this.options = options.Value;
            listener = new TcpListener(this.options.IPAddress, (int)this.options.Port);
        }

        /// <inheritdoc />
        public int Port => (int)options.Port;

        /// <inheritdoc />
        public void Start()
        {
            listener.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            listener.Stop();
        }

        /// <inheritdoc />
        public async Task<ITcpClient> Accept()
        {
            var client = await listener.AcceptTcpClientAsync();
            return new DefaultTcpClient(client);
        }
    }
}
