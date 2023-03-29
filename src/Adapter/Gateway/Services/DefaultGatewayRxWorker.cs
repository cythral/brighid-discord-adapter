using System.Collections.Generic;
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
        private ulong streamLength;

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
        public Dictionary<int, Task> TaskQueue { get; private set; } = new Dictionary<int, Task>();

        /// <inheritdoc />
        public async Task Start(IGatewayService gateway)
        {
            this.gateway = gateway;
            IsRunning = true;
            TaskQueue.Clear();

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
            await Task.WhenAll(TaskQueue.Values);
            await stream!.DisposeAsync();

            stream = null;
            worker = null;
            TaskQueue.Clear();
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
            streamLength += (ulong)chunk.Count;

            if (chunk.EndOfMessage && streamLength > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                stream.Position = 0;

                var copy = new MemoryStream();
                await stream.CopyToAsync(copy, cancellationToken);

                var task = ProcessMessage(copy, cancellationToken).ContinueWith(CleanupTask, CancellationToken.None);
                TaskQueue.Add(task.Id, task);

                stream.SetLength(0);
                streamLength = 0;
            }
        }

        private async Task ProcessMessage(Stream stream, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            stream.Position = 0;

            var message = await serializer.Deserialize<GatewayMessage>(stream, cancellationToken);
            await stream.DisposeAsync();

            logger.LogDebug("Received message from gateway: {@message}", message);
            gateway!.SequenceNumber = (message.SequenceNumber != null) ? message.SequenceNumber : gateway!.SequenceNumber;

            if (message.Data != null)
            {
                await eventRouter.Route(message.Data, cancellationToken);
            }
        }

        private void CleanupTask(Task task)
        {
            TaskQueue.Remove(task.Id);
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
