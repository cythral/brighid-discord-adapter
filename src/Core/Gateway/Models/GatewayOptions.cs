using System;

namespace Brighid.Discord.Gateway
{
    /// <summary>
    /// Options to use for the default gateway service.
    /// </summary>
    public class GatewayOptions
    {
        /// <summary>
        /// Gets or sets the uri of the websocket.
        /// </summary>
        public Uri Uri { get; set; } = new Uri("ws://localhost:8080");

        /// <summary>
        /// Gets or sets the size of the buffer to use.
        /// </summary>
        public uint BufferSize { get; set; } = 512;
    }
}
