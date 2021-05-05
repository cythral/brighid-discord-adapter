using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.GatewayAdapter.Messages;
using Brighid.Discord.Serialization;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.GatewayAdapter.Gateway
{
    /// <inheritdoc />
    [LogCategory(WorkerThreadName)]
    public partial class DefaultGatewayTxWorker : IGatewayTxWorker
    {
        private const string WorkerThreadName = "Gateway TX";
        private readonly IChannel<GatewayMessage> channel;
        private readonly ISerializer serializer;
        private readonly IGatewayUtilsFactory gatewayUtilsFactory;
        private readonly ILogger<DefaultGatewayTxWorker> logger;
        private CancellationToken cancellationToken;
        private IWorkerThread? workerThread;
        private IClientWebSocket? webSocket;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayTxWorker" /> class.
        /// </summary>
        /// <param name="serializer">Serializer used for deserializing messages from the gateway.</param>
        /// <param name="gatewayUtilsFactory">Factory to create various utils with.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public DefaultGatewayTxWorker(
            ISerializer serializer,
            IGatewayUtilsFactory gatewayUtilsFactory,
            ILogger<DefaultGatewayTxWorker> logger
        )
        {
            this.serializer = serializer;
            this.logger = logger;
            this.gatewayUtilsFactory = gatewayUtilsFactory;
            channel = gatewayUtilsFactory.CreateChannel<GatewayMessage>();
        }

        /// <inheritdoc />
        public void Start(IGatewayService gateway, IClientWebSocket webSocket, CancellationTokenSource cancellationTokenSource)
        {
            cancellationToken = cancellationTokenSource.Token;
            this.webSocket = webSocket;
            workerThread = gatewayUtilsFactory.CreateWorkerThread(Run, WorkerThreadName);
            workerThread.OnUnexpectedStop = () => gateway.Restart();
            workerThread.Start(cancellationTokenSource);
        }

        /// <inheritdoc />
        public void Stop()
        {
            workerThread?.Stop();
            workerThread = null;
        }

        /// <inheritdoc />
        public async Task Emit(GatewayMessage message, CancellationToken cancellationToken)
        {
            this.cancellationToken.ThrowIfCancellationRequested();
            cancellationToken.ThrowIfCancellationRequested();
            await channel.Write(message, cancellationToken);
        }

        /// <summary>
        /// Runs the Gateway TX Worker.
        /// </summary>
        /// <returns>The resulting task.</returns>
        public async Task Run()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (webSocket?.State != WebSocketState.Open)
                {
                    await gatewayUtilsFactory.CreateDelay(100, cancellationToken);
                }

                if (!await channel.WaitToRead(cancellationToken))
                {
                    continue;
                }

                var message = await channel.Read(cancellationToken);
                var serializedMessage = await serializer.SerializeToBytes(message, cancellationToken);

                logger.LogInformation("Sending Message: {@message}", message);
                await webSocket!.Send(serializedMessage, WebSocketMessageType.Text, true, cancellationToken);
            }
        }
    }
}
