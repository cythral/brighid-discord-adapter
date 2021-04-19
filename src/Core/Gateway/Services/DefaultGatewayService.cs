using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Events;
using Brighid.Discord.Messages;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Gateway
{
    /// <inheritdoc />
    [LogCategory(WorkerThreadName)]
    public class DefaultGatewayService : IGatewayService
    {
        private const string WorkerThreadName = "Gateway Master";
        private readonly GatewayOptions options;
        private readonly IGatewayRxWorker rxWorker;
        private readonly IGatewayTxWorker txWorker;
        private readonly IGatewayUtilsFactory gatewayUtilsFactory;
        private readonly ILogger<DefaultGatewayService> logger;
        private readonly byte[] buffer;
        private readonly Memory<byte> memoryBuffer;
        private IClientWebSocket? webSocket;
        private CancellationToken cancellationToken = new(true);
        private CancellationTokenSource? heartbeatCancellationTokenSource;
        private IWorkerThread? workerThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayService" /> class.
        /// </summary>
        /// <param name="rxWorker">The worker to use for receiving message chunks and parsing messages.</param>
        /// <param name="txWorker">The worker to use for sending messages to the gateway.</param>
        /// <param name="gatewayUtilsFactory">Factory to create various utils with.</param>
        /// <param name="options">Options to use for interacting with the gateway.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public DefaultGatewayService(
            IGatewayRxWorker rxWorker,
            IGatewayTxWorker txWorker,
            IGatewayUtilsFactory gatewayUtilsFactory,
            IOptions<GatewayOptions> options,
            ILogger<DefaultGatewayService> logger
        )
        {
            this.rxWorker = rxWorker;
            this.txWorker = txWorker;
            this.gatewayUtilsFactory = gatewayUtilsFactory;
            this.options = options.Value;
            this.logger = logger;
            buffer = new byte[this.options.BufferSize];
            memoryBuffer = new Memory<byte>(buffer);
        }

        /// <inheritdoc />
        public int? SequenceNumber { get; set; }

        /// <inheritdoc />
        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            cancellationToken = cancellationTokenSource.Token;
            workerThread = gatewayUtilsFactory.CreateWorkerThread(Run, WorkerThreadName);
            webSocket = gatewayUtilsFactory.CreateWebSocketClient();
            rxWorker.Start(this, cancellationTokenSource);
            txWorker.Start(webSocket, cancellationTokenSource);
            workerThread.Start(cancellationTokenSource);
        }

        /// <inheritdoc />
        public void Stop()
        {
            StopHeartbeat();
            workerThread?.Stop();
            rxWorker.Stop();
            txWorker.Stop();
            webSocket?.Abort();
            webSocket = null;
            workerThread = null;
        }

        /// <inheritdoc />
        public async Task Send(GatewayMessage message, CancellationToken cancellationToken)
        {
            this.cancellationToken.ThrowIfCancellationRequested();
            cancellationToken.ThrowIfCancellationRequested();
            await txWorker.Emit(message, cancellationToken);
        }

        /// <inheritdoc />
        public void StartHeartbeat(uint heartbeatInterval)
        {
            logger.LogInformation("Starting Heartbeat. Interval: {@heartbeatInterval}", heartbeatInterval);
            cancellationToken.ThrowIfCancellationRequested();
            heartbeatCancellationTokenSource = new CancellationTokenSource();
            _ = Heartbeat(heartbeatInterval, heartbeatCancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public void StopHeartbeat()
        {
            logger.LogInformation("Stopping Heartbeat. Last Sequence: {@sequenceNumber}", SequenceNumber);
            heartbeatCancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Runs the gateway service.
        /// </summary>
        /// <returns>The resulting task.</returns>
        public async Task Run()
        {
            cancellationToken.ThrowIfCancellationRequested();
            await webSocket!.Connect(options.Uri, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await webSocket!.Receive(memoryBuffer, cancellationToken);
                var chunk = new GatewayMessageChunk(memoryBuffer, result.Count, result.EndOfMessage);
                await rxWorker.Emit(chunk, cancellationToken);
            }
        }

        private async Task Heartbeat(uint heartbeatInterval, CancellationToken cancellationToken)
        {
            while (!this.cancellationToken.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                await gatewayUtilsFactory.CreateDelay(heartbeatInterval, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                var message = new GatewayMessage { OpCode = GatewayOpCode.Heartbeat, Data = (HeartbeatEvent?)SequenceNumber };
                await txWorker.Emit(message, cancellationToken);
            }
        }
    }
}
