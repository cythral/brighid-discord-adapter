using System;
using System.Net.Http;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Gateway API Endpoints.
    /// </summary>
    [Flags]
    [ApiCategory('g')]
    public enum GatewayEndpoint : ulong
    {
        /// <summary>
        /// Endpoint to post a message to a channel.
        /// </summary>
        [ApiEndpoint(nameof(HttpMethod.Get), "/gateway")]
        GetGateway = 1,

        /// <summary>
        /// Endpoint to post a message to a channel.
        /// </summary>
        [ApiEndpoint(nameof(HttpMethod.Get), "/gateway/bot")]
        GetGatewayBot = 2,
    }
}
