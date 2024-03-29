using System;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Events;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Models;
using Brighid.Discord.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable IDE0044

namespace Brighid.Discord.Adapter.Gateway
{
    /// <inheritdoc />
    [LogCategory(WorkerThreadName)]
    public class DefaultGatewayService : IGatewayService
    {
        private const string WorkerThreadName = "Gateway Master";
        private readonly GatewayOptions options;
        private readonly IGatewayRxWorker rxWorker;
        private readonly IGatewayTxWorker txWorker;
        private readonly ITimerFactory timerFactory;
        private readonly IGatewayRestartService restartService;
        private readonly IGatewayUtilsFactory gatewayUtilsFactory;
        private readonly IGatewayMetadataService metadataService;
        private readonly ILogger<DefaultGatewayService> logger;
        private readonly byte[] buffer;
        private readonly Memory<byte> memoryBuffer;
        private IClientWebSocket? webSocket;
        private ITimer? heartbeat;
        private ITimer? worker;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayService" /> class.
        /// </summary>
        /// <param name="rxWorker">The worker to use for receiving message chunks and parsing messages.</param>
        /// <param name="txWorker">The worker to use for sending messages to the gateway.</param>
        /// <param name="metadataService">Service for getting/setting gateway metadata.</param>
        /// <param name="timerFactory">Factory to create timers with.</param>
        /// <param name="restartService">Service used to restart the gateway.</param>
        /// <param name="gatewayUtilsFactory">Factory to create various utils with.</param>
        /// <param name="options">Options to use for interacting with the gateway.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public DefaultGatewayService(
            IGatewayRxWorker rxWorker,
            IGatewayTxWorker txWorker,
            IGatewayMetadataService metadataService,
            ITimerFactory timerFactory,
            IGatewayRestartService restartService,
            IGatewayUtilsFactory gatewayUtilsFactory,
            IOptions<GatewayOptions> options,
            ILogger<DefaultGatewayService> logger
        )
        {
            this.rxWorker = rxWorker;
            this.txWorker = txWorker;
            this.metadataService = metadataService;
            this.timerFactory = timerFactory;
            this.restartService = restartService;
            this.gatewayUtilsFactory = gatewayUtilsFactory;
            this.options = options.Value;
            this.logger = logger;
            buffer = new byte[this.options.BufferSize];
            memoryBuffer = new Memory<byte>(buffer);
        }

        /// <inheritdoc />
        public int RxTaskCount => rxWorker.TaskQueue.Count;

        /// <inheritdoc />
        public int? SequenceNumber { get; set; }

        /// <inheritdoc />
        public string? SessionId { get; set; }

        /// <inheritdoc />
        public Snowflake? BotId { get; set; }

        /// <inheritdoc />
        public GatewayState State { get; private set; }

        /// <inheritdoc/>
        public bool AwaitingHeartbeatAcknowledgement { get; set; }

        /// <inheritdoc />
        public void SetReadyState(bool ready)
        {
            if (!ready)
            {
                State &= ~GatewayState.Ready;
                return;
            }

            State |= GatewayState.Ready;
        }

        /// <inheritdoc/>
        public void ThrowIfNotReady()
        {
            if (!State.HasFlag(GatewayState.Ready))
            {
                throw new OperationCanceledException();
            }
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            State |= GatewayState.Running;
            webSocket = gatewayUtilsFactory.CreateWebSocketClient();

            worker = timerFactory.CreateTimer(Run, 0, WorkerThreadName);
            worker.StopOnException = true;
            worker.OnUnexpectedStop = () => Restart();
            worker.OnTimerStart = OnTimerStart;
            await worker.Start(cancellationToken);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            State &= ~GatewayState.Running;
            await StopHeartbeat();
            await StopWorker();

            await rxWorker.Stop();
            await txWorker.Stop();
            webSocket?.Abort();
            webSocket = null;
            State &= ~GatewayState.Ready;
        }

        /// <inheritdoc />
        public async Task Restart(bool resume = true, CancellationToken cancellationToken = default)
        {
            await restartService.Restart(this, resume, cancellationToken);
        }

        /// <inheritdoc />
        public async Task Send(GatewayMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfNotRunning();
            await txWorker.Emit(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task StartHeartbeat(uint heartbeatInterval)
        {
            ThrowIfNotRunning();
            logger.LogInformation("Starting Heartbeat. Interval: {@heartbeatInterval}", heartbeatInterval);
            heartbeat = timerFactory.CreateTimer(Heartbeat, (int)heartbeatInterval, "Heartbeat");
            heartbeat.StopOnException = true;
            heartbeat.OnUnexpectedStop = () => Restart();
            await heartbeat.Start(CancellationToken.None);
        }

        /// <inheritdoc />
        public async Task StopHeartbeat()
        {
            if (heartbeat == null)
            {
                return;
            }

            logger.LogInformation("Stopping Heartbeat. Last Sequence: {@sequenceNumber}", SequenceNumber);
            AwaitingHeartbeatAcknowledgement = false;
            await heartbeat.Stop();
        }

        /// <summary>
        /// Performs initialization needed to run the gateway, including waiting for kestrel to become ready, as well as connect to the websocket.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task OnTimerStart(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await gatewayUtilsFactory.CreateApplicationStartupDelay(cancellationToken);

            var gatewayUrl = await metadataService.GetGatewayUrl(cancellationToken);
            logger.LogInformation("Connecting to Discord with Gateway URL: {@gatewayUrl}", gatewayUrl);

            await webSocket!.Connect(gatewayUrl, cancellationToken);
            await rxWorker.Start(this, cancellationToken);
            await txWorker.Start(this, webSocket, cancellationToken);
        }

        /// <summary>
        /// Runs the gateway service.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task Run(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfNotRunning();

            var result = await webSocket!.Receive(memoryBuffer, cancellationToken);
            var chunk = new GatewayMessageChunk(memoryBuffer, result.Count, result.EndOfMessage);
            await rxWorker.Emit(chunk, cancellationToken);
        }

        /// <summary>
        /// Sends a Heartbeat through the gateway.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task Heartbeat(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (AwaitingHeartbeatAcknowledgement)
            {
                throw new Exception("Last heartbeat was not acknowledged.");
            }

            var message = new GatewayMessage { OpCode = GatewayOpCode.Heartbeat, Data = (HeartbeatEvent?)SequenceNumber };
            AwaitingHeartbeatAcknowledgement = true;
            await txWorker.Emit(message, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfNotRunning()
        {
            if (!State.HasFlag(GatewayState.Running) || webSocket?.State != WebSocketState.Open)
            {
                throw new OperationCanceledException();
            }
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
    }
}
