using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.RestClient.Responses;
using Brighid.Discord.Serialization;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.RestClient.Client
{
    /// <inheritdoc />
    public class DefaultDiscordRequestHandler : IDiscordRequestHandler
    {
        private readonly ISerializer serializer;
        private readonly IResponseService responseService;
        private readonly IRequestQueuer queuer;
        private readonly ILogger<DefaultDiscordRequestHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDiscordRequestHandler"/> class.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize/deserialize data between formats.</param>
        /// <param name="responseService">Service that listens for responses to requests.</param>
        /// <param name="queuer">Service used to queue requests.</param>
        /// <param name="logger">Logger used for logging info to some destination(s).</param>
        public DefaultDiscordRequestHandler(
            ISerializer serializer,
            IResponseService responseService,
            IRequestQueuer queuer,
            ILogger<DefaultDiscordRequestHandler> logger
        )
        {
            this.serializer = serializer;
            this.responseService = responseService;
            this.queuer = queuer;
            this.logger = logger;
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
                ResponseURL = responseService.Uri,
            };

            logger.LogDebug("Performing Discord API request with requestId: {@requestId} and response URL: {@url}", requestId, responseService.Uri);
            await queuer.QueueRequest(payload, cancellationToken);
            var response = await responseService.ListenForResponse(requestId, promise);
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

            logger.LogDebug("Performing Discord API request with requestId: {@requestId}", requestId);
            await queuer.QueueRequest(payload, cancellationToken);
        }
    }
}
