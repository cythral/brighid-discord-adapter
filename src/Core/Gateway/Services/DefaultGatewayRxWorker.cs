using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Messages;
using Brighid.Discord.Serialization;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Gateway
{
    /// <inheritdoc />
    public partial class DefaultGatewayRxWorker : IGatewayRxWorker
    {
        private const string workerName = "Gateway RX";
        private readonly IChannel<GatewayMessageChunk> channel;
        private readonly ISerializer serializer;
        private readonly IGatewayUtilsFactory gatewayUtilsFactory;
        private readonly ILogger<DefaultGatewayRxWorker> logger;
        private CancellationToken cancellationToken;
        private IWorkerThread? workerThread;
        private IGatewayService? gateway;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayRxWorker" /> class.
        /// </summary>
        /// <param name="serializer">Serializer to use for serializing messages.</param>
        /// <param name="gatewayUtilsFactory">Factory to create various utils with.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public DefaultGatewayRxWorker(
            ISerializer serializer,
            IGatewayUtilsFactory gatewayUtilsFactory,
            ILogger<DefaultGatewayRxWorker> logger
        )
        {
            this.serializer = serializer;
            this.gatewayUtilsFactory = gatewayUtilsFactory;
            this.logger = logger;
            channel = gatewayUtilsFactory.CreateChannel<GatewayMessageChunk>();
        }

        /// <inheritdoc />
        public void Start(IGatewayService gateway, CancellationTokenSource cancellationTokenSource)
        {
            cancellationToken = cancellationTokenSource.Token;
            this.gateway = gateway;
            workerThread = gatewayUtilsFactory.CreateWorkerThread(Run, workerName);
            workerThread.Start(cancellationTokenSource);
        }

        /// <inheritdoc />
        public void Stop()
        {
            workerThread?.Stop();
            workerThread = null;
        }

        /// <inheritdoc />
        public async Task Emit(GatewayMessageChunk chunk, CancellationToken cancellationToken)
        {
            this.cancellationToken.ThrowIfCancellationRequested();
            cancellationToken.ThrowIfCancellationRequested();
            await channel.Write(chunk, cancellationToken);
        }

        /// <summary>
        /// Runs the Rx Worker.
        /// </summary>
        /// <returns>The resulting task.</returns>
        public async Task Run()
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = gatewayUtilsFactory.CreateStream();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!await channel.WaitToRead(cancellationToken))
                {
                    continue;
                }

                var chunk = await channel.Read(cancellationToken);
                await stream.WriteAsync(chunk.Bytes, cancellationToken);

                if (chunk.EndOfMessage)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    stream.Position = 0;

                    var message = await serializer.Deserialize<GatewayMessage>(stream, cancellationToken);
                    gateway!.SequenceNumber = message.SequenceNumber;
                    message.Data?.Handle(gateway!, cancellationToken);

                    logger.LogInformation("{@workerName} Received message: {@message}", workerName, message);
                    stream.SetLength(0);
                }
            }
        }
    }
}
