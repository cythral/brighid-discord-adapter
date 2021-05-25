using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.Models.Payloads;
using Brighid.Discord.RestClient.Responses;
using Brighid.Discord.Serialization;

namespace Brighid.Discord.RestClient.Client
{
    /// <inheritdoc />
    public class DefaultDiscordUserClient : IDiscordUserClient
    {
        private readonly IResponseServer server;
        private readonly IRequestQueuer queuer;
        private readonly ISerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDiscordUserClient" /> class.
        /// </summary>
        /// <param name="server">The response server to listen to responses with.</param>
        /// <param name="queuer">Service used to queue requests.</param>
        /// <param name="serializer">Serializer used to serialize data between formats.</param>
        public DefaultDiscordUserClient(
            IResponseServer server,
            IRequestQueuer queuer,
            ISerializer serializer
        )
        {
            this.server = server;
            this.queuer = queuer;
            this.serializer = serializer;
        }

        /// <inheritdoc />
        public async Task<Channel> CreateDirectMessageChannel(Snowflake recipientId, CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();
            var promise = new TaskCompletionSource<Response>();
            var payload = new CreateDirectMessageChannelPayload { RecipientId = recipientId };
            var request = new Request(UserEndpoint.CreateDirectMessageChannel)
            {
                Id = requestId,
                RequestBody = await serializer.Serialize(payload, cancellationToken),
                ResponseURL = server.Uri,
            };

            await queuer.QueueRequest(request, cancellationToken);
            var response = await server.ListenForResponse(requestId, promise);
            return await serializer.Deserialize<Channel>(response.Body ?? "{}", cancellationToken);
        }
    }
}
