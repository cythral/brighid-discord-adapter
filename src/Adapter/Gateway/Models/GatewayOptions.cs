using System;
using System.Diagnostics.CodeAnalysis;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <summary>
    /// Options to use for the default gateway service.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
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

        /// <summary>
        /// Gets or sets the library name.
        /// </summary>
        public string LibraryName { get; set; } = "Brighid";
    }
}
