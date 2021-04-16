using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Gateway
{
    /// <summary>
    /// A client WebSocket object.
    /// </summary>
    public interface IClientWebSocket : IDisposable
    {
        /// <summary>
        /// Gets the WebSocket state of the System.Net.WebSockets.ClientWebSocket instance.
        /// </summary>
        WebSocketState State { get; }

        /// <summary>
        /// Connects to the web socket server located at <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The uri of the web socket server.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Connect(Uri uri, CancellationToken cancellationToken);

        /// <summary>
        /// Receives data through the web socket connection.
        /// </summary>
        /// <param name="buffer">The region of memory to receive the response.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        ValueTask<ValueWebSocketReceiveResult> Receive(Memory<byte> buffer, CancellationToken cancellationToken);

        /// <summary>
        /// Sends data through the web socket connection.
        /// </summary>
        /// <param name="buffer">The buffer containing the message to be sent.</param>
        /// <param name="messageType">One of the enumeration values that specifies whether the buffer is clear text or in a binary format.</param>
        /// <param name="endOfMessage">True to indicate this is the final asynchronous send; otherwise, false.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Send(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);

        /// <summary>
        /// Aborts the connection and cancels any pending IO operations.
        /// </summary>
        void Abort();
    }
}
