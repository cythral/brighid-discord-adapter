using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.RestClient.Client
{
    /// <summary>
    /// Client for discord gateway API endpoints.
    /// </summary>
    public interface IDiscordGatewayClient
    {
        /// <summary>
        /// Gets information about the discord gateway.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Information about the discord gateway.</returns>
        Task<GatewayInfo> GetGatewayInfo(CancellationToken cancellationToken);

        /// <summary>
        /// Gets information about the discord gateway, plus info pertinent to bots.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Info about the discord gateway, with metadata for bots.</returns>
        Task<GatewayBotInfo> GetGatewayBotInfo(CancellationToken cancellationToken);
    }
}
