using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Events;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Serialization;
using Brighid.Discord.Threading;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <inheritdoc />
    [LogCategory(WorkerThreadName)]
    public partial class DefaultGatewayRxWorker : IGatewayRxWorker
    {
        private const string WorkerThreadName = "Gateway RX";
        private readonly IChannel<GatewayMessageChunk> channel;
        private readonly ISerializer serializer;
        private readonly IGatewayUtilsFactory gatewayUtilsFactory;
        private readonly IEventRouter eventRouter;
        private readonly ILogger<DefaultGatewayRxWorker> logger;
        private readonly ITimerFactory timerFactory;
        private ITimer? worker;
        private IGatewayService? gateway;
        private Stream? stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayRxWorker" /> class.
        /// </summary>
        /// <param name="serializer">Serializer to use for serializing messages.</param>
        /// <param name="timerFactory">Factory used to create timers.</param>
        /// <param name="gatewayUtilsFactory">Factory to create various utils with.</param>
        /// <param name="eventRouter">Router to route events to controllers.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public DefaultGatewayRxWorker(
            ISerializer serializer,
            ITimerFactory timerFactory,
            IGatewayUtilsFactory gatewayUtilsFactory,
            IEventRouter eventRouter,
            ILogger<DefaultGatewayRxWorker> logger
        )
        {
            this.serializer = serializer;
            this.timerFactory = timerFactory;
            this.gatewayUtilsFactory = gatewayUtilsFactory;
            this.eventRouter = eventRouter;
            this.logger = logger;
            channel = gatewayUtilsFactory.CreateChannel<GatewayMessageChunk>();
        }

        /// <inheritdoc />
        public bool IsRunning { get; private set; }

        /// <inheritdoc />
        public async Task Start(IGatewayService gateway)
        {
            this.gateway = gateway;
            IsRunning = true;

            stream = gatewayUtilsFactory.CreateStream();
            worker = timerFactory.CreateTimer(Run, 0, WorkerThreadName);
            worker.StopOnException = true;
            worker.OnUnexpectedStop = () => gateway.Restart();
            await worker.Start();
        }

        /// <inheritdoc />
        public async Task Stop()
        {
            IsRunning = false;
            await worker!.Stop();
            await stream!.DisposeAsync();
            stream = null;
            worker = null;
        }

        /// <inheritdoc />
        public async Task Emit(GatewayMessageChunk chunk, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfNotRunning();
            await channel.Write(chunk, cancellationToken);
        }

        /// <summary>
        /// Runs the Rx Worker.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task Run(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfNotRunning();

            if (!await channel.WaitToRead(cancellationToken))
            {
                return;
            }

            var chunk = await channel.Read(cancellationToken);
            await stream!.WriteAsync(chunk.Bytes, cancellationToken);

            if (chunk.EndOfMessage)
            {
                cancellationToken.ThrowIfCancellationRequested();
                stream.Position = 0;

                var message = await serializer.Deserialize<GatewayMessage>(stream, cancellationToken);
                if (message.SequenceNumber != null)
                {
                    gateway!.SequenceNumber = message.SequenceNumber;
                }

                if (message.Data != null)
                {
                    _ = eventRouter.Route(message.Data, cancellationToken);
                }

                logger.LogDebug("Received message from gateway: {@message}", message);
                stream.SetLength(0);
            }
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
