using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.RestClient.Responses;
using Brighid.Discord.Serialization;
using Brighid.Discord.Tracing;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.RestClient.Client
{
    /// <inheritdoc />
    public class DefaultDiscordRequestHandler : IDiscordRequestHandler
    {
        private readonly ISerializer serializer;
        private readonly IResponseService responseService;
        private readonly IRequestQueuer queuer;
        private readonly ITracingService tracing;
        private readonly ILogger<DefaultDiscordRequestHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDiscordRequestHandler"/> class.
        /// </summary>
        /// <param name="serializer">Serializer used to serialize/deserialize data between formats.</param>
        /// <param name="responseService">Service that listens for responses to requests.</param>
        /// <param name="queuer">Service used to queue requests.</param>
        /// <param name="tracing">Service for tracing requests across microservices.</param>
        /// <param name="logger">Logger used for logging info to some destination(s).</param>
        public DefaultDiscordRequestHandler(
            ISerializer serializer,
            IResponseService responseService,
            IRequestQueuer queuer,
            ITracingService tracing,
            ILogger<DefaultDiscordRequestHandler> logger
        )
        {
            this.serializer = serializer;
            this.responseService = responseService;
            this.queuer = queuer;
            this.tracing = tracing;
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
            var requestBody = serializer.Serialize(request);
            var requestId = await Handle(endpoint, true, requestBody, parameters, headers, cancellationToken);
            return await WaitForResponse<TResponse>(requestId, cancellationToken);
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
            cancellationToken.ThrowIfCancellationRequested();
            var requestBody = serializer.Serialize(request);
            await Handle(endpoint, false, requestBody, parameters, headers, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TResponse?> Handle<TResponse>(
            Endpoint endpoint,
            Dictionary<string, string>? parameters = null,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestId = await Handle(endpoint, true, null, parameters, headers, cancellationToken);
            return await WaitForResponse<TResponse>(requestId, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<Guid> Handle(
            Endpoint endpoint,
            bool expectResponse,
            string? request,
            Dictionary<string, string>? parameters = null,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var requestId = Guid.NewGuid();
            var payload = new Request(endpoint)
            {
                Id = requestId,
                Parameters = parameters ?? new Dictionary<string, string>(),
                TraceHeader = tracing.Header,
                Headers = headers ?? new Dictionary<string, string>(),
                ResponseURL = expectResponse ? responseService.Uri : null,
                RequestBody = request,
            };

            logger.LogDebug("Performing Discord API request with requestId: {@requestId} and response URL: {@url}", requestId, responseService.Uri);
            await queuer.QueueRequest(payload, cancellationToken);
            return requestId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task<TResponse?> WaitForResponse<TResponse>(Guid requestId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var promise = new TaskCompletionSource<Response>();
            var response = await responseService.ListenForResponse(requestId, promise, cancellationToken);
            return serializer.Deserialize<TResponse>(response.Body ?? "{}");
        }
    }
}
