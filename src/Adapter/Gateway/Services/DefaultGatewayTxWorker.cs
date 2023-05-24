using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Serialization;
using Brighid.Discord.Threading;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <inheritdoc />
    [LogCategory(WorkerThreadName)]
    public partial class DefaultGatewayTxWorker : IGatewayTxWorker
    {
        private const string WorkerThreadName = "Gateway TX";
        private readonly IChannel<GatewayMessage> channel;
        private readonly ISerializer serializer;
        private readonly ITimerFactory timerFactory;
        private readonly IGatewayUtilsFactory gatewayUtilsFactory;
        private readonly ILogger<DefaultGatewayTxWorker> logger;
        private ITimer? worker;
        private IClientWebSocket? webSocket;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayTxWorker" /> class.
        /// </summary>
        /// <param name="serializer">Serializer used for deserializing messages from the gateway.</param>
        /// <param name="timerFactory">Factory used to create timers.</param>
        /// <param name="gatewayUtilsFactory">Factory to create various utils with.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public DefaultGatewayTxWorker(
            ISerializer serializer,
            ITimerFactory timerFactory,
            IGatewayUtilsFactory gatewayUtilsFactory,
            ILogger<DefaultGatewayTxWorker> logger
        )
        {
            this.serializer = serializer;
            this.timerFactory = timerFactory;
            this.logger = logger;
            this.gatewayUtilsFactory = gatewayUtilsFactory;
            channel = gatewayUtilsFactory.CreateChannel<GatewayMessage>();
        }

        /// <inheritdoc />
        public bool IsRunning { get; private set; }

        /// <inheritdoc />
        public async Task Start(IGatewayService gateway, IClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Starting Gateway TX Worker.");
            IsRunning = true;
            this.webSocket = webSocket;

            worker = timerFactory.CreateTimer(Run, 0, WorkerThreadName);
            worker.StopOnException = true;
            worker.OnUnexpectedStop = () => gateway.Restart();
            await worker.Start(cancellationToken);
            logger.LogInformation("Gateway TX Worker became ready.");
        }

        /// <inheritdoc />
        public async Task Stop()
        {
            IsRunning = false;
            await StopWorker();
        }

        /// <inheritdoc />
        public async Task Emit(GatewayMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfNotRunning();
            await channel.Write(message, cancellationToken);
        }

        /// <summary>
        /// Runs the Gateway TX Worker.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task Run(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfNotRunning();

            while (webSocket?.State != WebSocketState.Open)
            {
                if (webSocket!.State > WebSocketState.Open)
                {
                    throw new System.Exception("WebSocket is not open.");
                }

                await gatewayUtilsFactory.CreateDelay(1000, cancellationToken);
            }

            if (!await channel.WaitToRead(cancellationToken))
            {
                return;
            }

            var message = await channel.Read(cancellationToken);
            var serializedMessage = serializer.SerializeToBytes(message);

            logger.LogDebug("Sending message to gateway: {@message}", message);
            await webSocket!.Send(serializedMessage, WebSocketMessageType.Text, true, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task StopWorker()
        {
            if (worker != null)
            {
                await worker.Stop();
            }

            worker = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfNotRunning()
        {
            if (!IsRunning)
            {
                throw new System.OperationCanceledException();
            }
        }
    }
}
