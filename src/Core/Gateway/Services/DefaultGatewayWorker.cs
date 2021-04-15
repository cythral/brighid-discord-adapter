using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Brighid.Discord.Messages;
using Brighid.Discord.Serialization;

namespace Brighid.Discord.Gateway
{
    /// <inheritdoc />
    public partial class DefaultGatewayWorker : IGatewayWorker
    {
        private readonly Thread thread;
        private readonly Channel<GatewayMessageChunk> channel;
        private readonly Stream stream;
        private readonly ISerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayWorker" /> class.
        /// </summary>
        /// <param name="serializer">serializer.</param>
        public DefaultGatewayWorker(
            ISerializer serializer
        )
        {
            this.serializer = serializer;
            channel = Channel.CreateUnbounded<GatewayMessageChunk>();
            thread = new Thread(Run);
            stream = new MemoryStream();
        }

        /// <inheritdoc />
        public void Start()
        {
            thread.Start();
        }

        /// <inheritdoc />
        public async Task Emit(GatewayMessageChunk chunk, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var writer = channel.Writer;
            await writer.WriteAsync(chunk, cancellationToken);
        }

        private void Run()
        {
            Task.WaitAll(RunAsync());
        }

        private async Task RunAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await channel.Reader.WaitToReadAsync(cancellationToken);
                var chunk = await channel.Reader.ReadAsync(cancellationToken);
                await stream.WriteAsync(chunk.Bytes, cancellationToken);

                if (chunk.EndOfMessage)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    stream.Position = 0;

                    var message = await serializer.Deserialize<GatewayMessage>(stream, cancellationToken);
                    Console.WriteLine(await serializer.Serialize(message, cancellationToken));
                    stream.SetLength(0);
                }
            }
        }
    }
}
