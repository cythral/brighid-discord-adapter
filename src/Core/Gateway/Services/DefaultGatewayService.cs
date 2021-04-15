using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Messages;

using Microsoft.Extensions.Options;

namespace Brighid.Discord.Gateway
{
    /// <inheritdoc />
    public class DefaultGatewayService : IGatewayService
    {
        private readonly ClientWebSocket webSocket;
        private readonly GatewayOptions options;
        private readonly IGatewayWorker worker;
        private readonly byte[] buffer;
        private readonly Memory<byte> memoryBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayService" /> class.
        /// </summary>
        /// <param name="worker">The worker to use for receiving message chunks and parsing messages.</param>
        /// <param name="options">Options to use for interacting with the gateway.</param>
        public DefaultGatewayService(IGatewayWorker worker, IOptions<GatewayOptions> options)
        {
            this.options = options.Value;
            this.worker = worker;
            webSocket = new ClientWebSocket();
            buffer = new byte[this.options.BufferSize];
            memoryBuffer = new Memory<byte>(buffer);
        }

        /// <inheritdoc />
        public async Task Run(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await webSocket.ConnectAsync(options.Uri, cancellationToken);
            worker.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(memoryBuffer, cancellationToken);
                var chunk = new GatewayMessageChunk(memoryBuffer, result.Count, result.EndOfMessage);
                await worker.Emit(chunk, cancellationToken);
            }
        }
    }
}
