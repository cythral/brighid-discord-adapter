using System;
using System.Net.WebSockets;
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
        /// <param name="timerFactory">Factory to create timers with.</param>
        /// <param name="restartService">Service used to restart the gateway.</param>
        /// <param name="gatewayUtilsFactory">Factory to create various utils with.</param>
        /// <param name="options">Options to use for interacting with the gateway.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public DefaultGatewayService(
            IGatewayRxWorker rxWorker,
            IGatewayTxWorker txWorker,
            ITimerFactory timerFactory,
            IGatewayRestartService restartService,
            IGatewayUtilsFactory gatewayUtilsFactory,
            IOptions<GatewayOptions> options,
            ILogger<DefaultGatewayService> logger
        )
        {
            this.rxWorker = rxWorker;
            this.txWorker = txWorker;
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

            var cancellationTokenSource = new CancellationTokenSource();
            await rxWorker.Start(this);
            await txWorker.Start(this, webSocket);

            worker = timerFactory.CreateTimer(Run, 0, WorkerThreadName);
            worker.StopOnException = true;
            worker.OnUnexpectedStop = () => Restart();
            await worker.Start();
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            State &= ~GatewayState.Running;
            await StopHeartbeat();
            await worker!.Stop();
            await rxWorker.Stop();
            await txWorker.Stop();
            webSocket?.Abort();
            webSocket = null;
            worker = null;
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
            await heartbeat.Start();
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
        /// Runs the gateway service.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task Run(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfNotRunning();
            if (webSocket!.State != WebSocketState.Open)
            {
                await webSocket!.Connect(options.Uri, cancellationToken);
            }

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
                await restartService.Restart(this, true, cancellationToken);
                return;
            }

            var message = new GatewayMessage { OpCode = GatewayOpCode.Heartbeat, Data = (HeartbeatEvent?)SequenceNumber };
            AwaitingHeartbeatAcknowledgement = true;
            await txWorker.Emit(message, cancellationToken);
        }

        private void ThrowIfNotRunning()
        {
            if (!State.HasFlag(GatewayState.Running))
            {
                throw new OperationCanceledException();
            }
        }
    }
}
