using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;

namespace Brighid.Discord.RestClient.Client
{
    /// <inheritdoc />
    public class DefaultDiscordUserClient : IDiscordUserClient
    {
        private readonly IDiscordRequestHandler handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDiscordUserClient" /> class.
        /// </summary>
        /// <param name="handler">Service used to handle requests.</param>
        public DefaultDiscordUserClient(
            IDiscordRequestHandler handler
        )
        {
            this.handler = handler;
        }

        /// <inheritdoc />
        public async Task<Channel> CreateDirectMessageChannel(Snowflake recipientId, CancellationToken cancellationToken = default)
        {
            var payload = new CreateDirectMessageChannelPayload { RecipientId = recipientId };
            return await handler.Handle<CreateDirectMessageChannelPayload, Channel>(UserEndpoint.CreateDirectMessageChannel, payload, cancellationToken: cancellationToken);
        }
    }
}
