using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.RestClient.Client
{
    /// <summary>
    /// Client to interact with user endpoints.
    /// </summary>
    public interface IDiscordUserClient
    {
        /// <summary>
        /// Create a new DM channel with a user.
        /// </summary>
        /// <param name="recipientId">The recipient to open a DM channel with.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting channel.</returns>
        Task<Channel> CreateDirectMessageChannel(Snowflake recipientId, CancellationToken cancellationToken = default);
    }
}
