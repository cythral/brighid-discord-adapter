using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Brighid.Discord.Messages;

namespace Brighid.Discord.Gateway
{
    /// <inheritdoc />
    public partial class DefaultGatewayWorker : IGatewayWorker
    {
        private readonly Thread thread;
        private readonly Channel<GatewayMessageChunk> channel;
        private readonly Stream stream;
        private readonly IMessageParser parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayWorker" /> class.
        /// </summary>
        /// <param name="parser">The parser used to parse messages/event data.</param>
        public DefaultGatewayWorker(
            IMessageParser parser
        )
        {
            this.parser = parser;
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

                    _ = await parser.Parse(stream, cancellationToken);
                    stream.SetLength(0);
                }
            }
        }
    }
}
