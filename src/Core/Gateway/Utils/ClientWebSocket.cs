using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using SystemClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Brighid.Discord.Gateway
{
    /// <inheritdoc />
    public sealed class ClientWebSocket : IClientWebSocket
    {
        private SystemClientWebSocket? webSocket = new();

        /// <inheritdoc />
        public WebSocketState State => webSocket?.State ?? throw new ObjectDisposedException(nameof(ClientWebSocket));

        /// <inheritdoc />
        public async Task Connect(Uri uri, CancellationToken cancellationToken)
        {
            if (webSocket == null)
            {
                throw new ObjectDisposedException(nameof(ClientWebSocket));
            }

            await webSocket.ConnectAsync(uri, cancellationToken);
        }

        /// <inheritdoc />
        public async ValueTask<ValueWebSocketReceiveResult> Receive(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            return webSocket != null
                ? await webSocket.ReceiveAsync(buffer, cancellationToken)
                : throw new ObjectDisposedException(nameof(ClientWebSocket));
        }

        /// <inheritdoc />
        public async Task Send(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            if (webSocket == null)
            {
                throw new ObjectDisposedException(nameof(ClientWebSocket));
            }

            await webSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
        }

        /// <inheritdoc />
        public void Abort()
        {
            if (webSocket == null)
            {
                throw new ObjectDisposedException(nameof(ClientWebSocket));
            }

            webSocket.Abort();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (webSocket == null)
            {
                throw new ObjectDisposedException(nameof(ClientWebSocket));
            }

            webSocket.Dispose();
            webSocket = null;
        }
    }
}
