using System.Net.WebSockets;
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
        private ITimer? timer;
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
        public async Task Start(IGatewayService gateway, IClientWebSocket webSocket)
        {
            IsRunning = true;
            this.webSocket = webSocket;

            timer = timerFactory.CreateTimer(Run, 0, WorkerThreadName);
            timer.StopOnException = true;
            timer.OnUnexpectedStop = () => gateway.Restart();
            await timer.Start();
        }

        /// <inheritdoc />
        public async Task Stop()
        {
            IsRunning = false;
            await timer!.Stop();
            timer = null;
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
                await gatewayUtilsFactory.CreateDelay(100, cancellationToken);
            }

            if (!await channel.WaitToRead(cancellationToken))
            {
                return;
            }

            var message = await channel.Read(cancellationToken);
            var serializedMessage = await serializer.SerializeToBytes(message, cancellationToken);

            logger.LogInformation("Sending Message: {@message}", message);
            await webSocket!.Send(serializedMessage, WebSocketMessageType.Text, true, cancellationToken);
        }

        private void ThrowIfNotRunning()
        {
            if (!IsRunning)
            {
                throw new System.OperationCanceledException();
            }
        }
    }
}
