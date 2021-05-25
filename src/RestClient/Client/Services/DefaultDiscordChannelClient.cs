using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;

namespace Brighid.Discord.RestClient.Client
{
    /// <inheritdoc />
    public class DefaultDiscordChannelClient : IDiscordChannelClient
    {
        private readonly IDiscordRequestHandler handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDiscordChannelClient"/> class.
        /// </summary>
        /// <param name="handler">Service used to handle requests to the Discord API.</param>
        public DefaultDiscordChannelClient(
            IDiscordRequestHandler handler
        )
        {
            this.handler = handler;
        }

        /// <inheritdoc />
        public async Task CreateMessage(Snowflake channelId, string message, CancellationToken cancellationToken = default)
        {
            var request = new CreateMessagePayload { Content = message };
            await handler.Handle(
                endpoint: ChannelEndpoint.CreateMessage,
                request: request,
                parameters: new Dictionary<string, string> { ["channel.id"] = channelId },
                cancellationToken: cancellationToken
            );
        }
    }
}
