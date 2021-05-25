using System.Net;

namespace Brighid.Discord.RestClient.Responses
{
    /// <summary>
    /// Options to use for the TCP Listener.
    /// </summary>
    public class TcpListenerOptions
    {
        /// <summary>
        /// Gets or sets the IP Address to bind to.
        /// </summary>
        public IPAddress IPAddress { get; set; } = IPAddress.Parse("0.0.0.0");

        /// <summary>
        /// Gets or sets the port to bind to.
        /// </summary>
        public uint Port { get; set; } = 19426;
    }
}
