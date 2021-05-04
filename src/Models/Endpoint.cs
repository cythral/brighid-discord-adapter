using System.Net.Http;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Represents an endpoint on the API that can be hit.
    /// </summary>
    public enum Endpoint
    {
        /// <summary>
        /// Endpoint to post a message to a channel.
        /// </summary>
        [ApiEndpoint(nameof(HttpMethod.Post), "/channels/{channel.id}/messages")]
        ChannelCreateMessage = 0,
    }
}
