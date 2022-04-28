using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;

namespace Brighid.Discord.RestClient.Client
{
    /// <summary>
    /// Client to interact with channel endpoints.
    /// </summary>
    public interface IDiscordChannelClient
    {
        /// <summary>
        /// Creates a message in a channel.
        /// </summary>
        /// <param name="channelId">ID of the channel to send the message to.</param>
        /// <param name="request">The message to send to the channel.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task CreateMessage(Snowflake channelId, CreateMessagePayload request, CancellationToken cancellationToken = default);
    }
}
