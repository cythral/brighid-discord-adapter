using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.RestClient.Client
{
    /// <inheritdoc />
    public class DefaultDiscordGatewayClient : IDiscordGatewayClient
    {
        private readonly IDiscordRequestHandler handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDiscordGatewayClient"/> class.
        /// </summary>
        /// <param name="handler">Service used to handle requests to the Discord API.</param>
        public DefaultDiscordGatewayClient(
            IDiscordRequestHandler handler
        )
        {
            this.handler = handler;
        }

        /// <inheritdoc />
        public async Task<GatewayInfo> GetGatewayInfo(CancellationToken cancellationToken)
        {
            return await handler.Handle<GatewayInfo>(GatewayEndpoint.GetGateway, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async Task<GatewayBotInfo> GetGatewayBotInfo(CancellationToken cancellationToken)
        {
            return await handler.Handle<GatewayBotInfo>(GatewayEndpoint.GetGatewayBot, cancellationToken: cancellationToken);
        }
    }
}
