using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.GatewayAdapter.Messages;

namespace Brighid.Discord.GatewayAdapter.Gateway
{
    /// <summary>
    /// A worker to send bytes through the websocket.
    /// </summary>
    public interface IGatewayTxWorker
    {
        /// <summary>
        /// Start the worker.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="webSocket">The webSocket to send messages to.</param>
        /// <param name="cancellationTokenSource">Source token used to cancel the worker's thread.</param>
        void Start(IGatewayService gateway, IClientWebSocket webSocket, CancellationTokenSource cancellationTokenSource);

        /// <summary>
        /// Stop the gateway worker.
        /// </summary>
        void Stop();

        /// <summary>
        /// Emit bytes to the worker for processing.
        /// </summary>
        /// <param name="message">The message to emit.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The created task.</returns>
        Task Emit(GatewayMessage message, CancellationToken cancellationToken = default);
    }
}
