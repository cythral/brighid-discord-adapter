using System;
using System.Net.Http;

#pragma warning disable SA1649

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Represents an endpoint on the API that can be hit.
    /// </summary>
    [Flags]
    [ApiCategory('c')]
    public enum ChannelEndpoint : ulong
    {
        /// <summary>
        /// Endpoint to post a message to a channel.
        /// </summary>
        [ApiEndpoint(nameof(HttpMethod.Post), "/channels/{channel.id}/messages")]
        CreateMessage = 1,

        /// <summary>
        /// Endpoint to delete a message from a channel.
        /// </summary>
        [ApiEndpoint(nameof(HttpMethod.Delete), "/channels/{channel.id}/messages/{message.id}")]
        DeleteMessage = 2,
    }
}
