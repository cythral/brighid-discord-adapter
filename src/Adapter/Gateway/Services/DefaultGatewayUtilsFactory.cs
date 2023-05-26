using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Threading;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <inheritdoc />
    [LogCategory("Gateway Utils")]
    public class DefaultGatewayUtilsFactory : IGatewayUtilsFactory
    {
        private readonly Random random;
        private readonly HttpClient httpClient;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<DefaultGatewayUtilsFactory> logger;
        private bool applicationStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayUtilsFactory" /> class.
        /// </summary>
        /// <param name="random">Random number generator.</param>
        /// <param name="httpClient">Client for making HTTP Requests.</param>
        /// <param name="loggerFactory">Logger factory for creating logger objects.</param>
        /// <param name="logger">Logger used for logging info to some destination(s).</param>
        public DefaultGatewayUtilsFactory(
            Random random,
            HttpClient httpClient,
            ILoggerFactory loggerFactory,
            ILogger<DefaultGatewayUtilsFactory> logger
        )
        {
            this.random = random;
            this.httpClient = httpClient;
            this.loggerFactory = loggerFactory;
            this.logger = logger;
        }

        /// <inheritdoc />
        public CancellationTokenSource CreateCancellationTokenSource()
        {
            return new CancellationTokenSource();
        }

        /// <inheritdoc />
        public IClientWebSocket CreateWebSocketClient()
        {
            return new ClientWebSocket();
        }

        /// <inheritdoc />
        public IWorkerThread CreateWorkerThread(Func<Task> runAsync, string workerName)
        {
            var logger = loggerFactory.CreateLogger(workerName);
            return new WorkerThread(runAsync, workerName, logger);
        }

        /// <inheritdoc />
        public IChannel<TMessage> CreateChannel<TMessage>()
        {
            return new Channel<TMessage>();
        }

        /// <inheritdoc />
        public async Task CreateApplicationStartupDelay(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (applicationStarted)
            {
                return;
            }

            for (var tries = 0; tries < 20; tries++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/healthcheck"),
                    Method = HttpMethod.Get,
                    Version = new Version(2, 0),
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact,
                };

                try
                {
                    var response = await httpClient.SendAsync(request, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    applicationStarted = true;
                    return;
                }
                catch (Exception)
                {
                }

                await Task.Delay(100, cancellationToken);
            }

            throw new Exception("Application failed to become ready.");
        }

        /// <inheritdoc />
        public Stream CreateStream()
        {
            return new MemoryStream();
        }

        /// <inheritdoc />
        public async Task CreateDelay(uint millisecondsDelay, CancellationToken cancellationToken)
        {
            logger.LogDebug("Sleeping for {timespan} ms", millisecondsDelay);
            await Task.Delay((int)millisecondsDelay, cancellationToken);
        }

        /// <inheritdoc />
        public async Task CreateRandomDelay(uint minimum, uint maximum, CancellationToken cancellationToken)
        {
            var delay = random.Next((int)minimum, (int)maximum);
            await CreateDelay((uint)delay, cancellationToken);
        }
    }
}
