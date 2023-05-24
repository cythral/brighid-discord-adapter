using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Messages;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <summary>
    /// A worker to send bytes through the websocket.
    /// </summary>
    public interface IGatewayTxWorker
    {
        /// <summary>
        /// Gets a value indicating whether the service is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Start the worker.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="webSocket">The webSocket to send messages to.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Start(IGatewayService gateway, IClientWebSocket webSocket, CancellationToken cancellationToken);

        /// <summary>
        /// Stop the gateway worker.
        /// </summary>
        /// <returns>The resulting task.</returns>
        Task Stop();

        /// <summary>
        /// Emit bytes to the worker for processing.
        /// </summary>
        /// <param name="message">The message to emit.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The created task.</returns>
        Task Emit(GatewayMessage message, CancellationToken cancellationToken = default);
    }
}
