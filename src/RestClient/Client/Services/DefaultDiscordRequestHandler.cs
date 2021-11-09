using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.RestClient.Responses;
using Brighid.Discord.Serialization;

namespace Brighid.Discord.RestClient.Client
{
    /// <inheritdoc />
    public class DefaultDiscordRequestHandler : IDiscordRequestHandler
    {
        private readonly ISerializer serializer;
        private readonly IResponseServer server;
        private readonly IRequestQueuer queuer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDiscordRequestHandler"/> class.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize/deserialize data between formats.</param>
        /// <param name="server">Server that listens for responses to requests.</param>
        /// <param name="queuer">Service used to queue requests.</param>
        public DefaultDiscordRequestHandler(
            ISerializer serializer,
            IResponseServer server,
            IRequestQueuer queuer
        )
        {
            this.serializer = serializer;
            this.server = server;
            this.queuer = queuer;
        }

        /// <inheritdoc />
        public async Task<TResponse?> Handle<TRequest, TResponse>(
            Endpoint endpoint,
            TRequest request,
            Dictionary<string, string>? parameters = null,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestId = Guid.NewGuid();
            var promise = new TaskCompletionSource<Response>();
            var payload = new Request(endpoint)
            {
                Id = requestId,
                Parameters = parameters ?? new Dictionary<string, string>(),
                Headers = headers ?? new Dictionary<string, string>(),
                RequestBody = serializer.Serialize(request),
                ResponseURL = server.Uri,
            };

            await queuer.QueueRequest(payload, cancellationToken);
            var response = await server.ListenForResponse(requestId, promise);
            return serializer.Deserialize<TResponse>(response.Body ?? "{}");
        }

        /// <inheritdoc />
        public async Task Handle<TRequest>(
            Endpoint endpoint,
            TRequest request,
            Dictionary<string, string>? parameters = null,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default
        )
        {
            var requestId = Guid.NewGuid();
            var payload = new Request(endpoint)
            {
                Id = requestId,
                Parameters = parameters ?? new Dictionary<string, string>(),
                Headers = headers ?? new Dictionary<string, string>(),
                RequestBody = serializer.Serialize(request),
            };

            await queuer.QueueRequest(payload, cancellationToken);
        }
    }
}
