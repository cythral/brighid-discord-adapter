using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Threading;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <inheritdoc />
    [LogCategory("Gateway Utils")]
    public class DefaultGatewayUtilsFactory : IGatewayUtilsFactory
    {
        private readonly Random random;
        private readonly ILoggerFactory loggerFactory;
        private readonly IHostApplicationLifetime lifetime;
        private readonly ILogger<DefaultGatewayUtilsFactory> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayUtilsFactory" /> class.
        /// </summary>
        /// <param name="random">Random number generator.</param>
        /// <param name="loggerFactory">Logger factory for creating logger objects.</param>
        /// <param name="lifetime">Service used to detect application lifetime changes.</param>
        /// <param name="logger">Logger used for logging info to some destination(s).</param>
        public DefaultGatewayUtilsFactory(
            Random random,
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime lifetime,
            ILogger<DefaultGatewayUtilsFactory> logger
        )
        {
            this.random = random;
            this.loggerFactory = loggerFactory;
            this.lifetime = lifetime;
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

            while (!lifetime.ApplicationStarted.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }
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
