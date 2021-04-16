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
        private readonly ILogger<DefaultGatewayUtilsFactory> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayUtilsFactory" /> class.
        /// </summary>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public DefaultGatewayUtilsFactory(ILogger<DefaultGatewayUtilsFactory> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public IClientWebSocket CreateWebSocketClient()
        {
            return new ClientWebSocket();
        }

        /// <inheritdoc />
        public IWorkerThread CreateWorkerThread(Func<Task> runAsync, string workerName)
        {
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
        public async Task CreateDelay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            await Task.Delay(millisecondsDelay, cancellationToken);
        }
    }
}
