using System;
using System.Net.Http;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Represents an endpoint on the API that can be hit.
    /// </summary>
    [Flags]
    [ApiCategory('u')]
    public enum UserEndpoint : ulong
    {
        /// <summary>
        /// Endpoint to post a message to a channel.
        /// </summary>
        [ApiEndpoint(nameof(HttpMethod.Post), "/users/@me/channels")]
        CreateDirectMessageChannel = 1,
    }
}
