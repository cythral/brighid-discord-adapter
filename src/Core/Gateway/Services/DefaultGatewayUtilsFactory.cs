using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Gateway
{
    /// <inheritdoc />
    public class DefaultGatewayUtilsFactory : IGatewayUtilsFactory
    {
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayUtilsFactory" /> class.
        /// </summary>
        /// <param name="loggerFactory">Logger factory for creating logger objects.</param>
        public DefaultGatewayUtilsFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
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
        public Stream CreateStream()
        {
            return new MemoryStream();
        }

        /// <inheritdoc />
        public async Task CreateDelay(uint millisecondsDelay, CancellationToken cancellationToken)
        {
            await Task.Delay((int)millisecondsDelay, cancellationToken);
        }
    }
}
