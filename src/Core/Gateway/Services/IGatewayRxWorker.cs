using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Messages;

namespace Brighid.Discord.Gateway
{
    /// <summary>
    /// A worker to receive bytes from a buffer and parse them into a message.
    /// </summary>
    public interface IGatewayRxWorker
    {
        /// <summary>
        /// Start the worker.
        /// </summary>
        /// <param name="gateway">The gateway to work on.</param>
        /// <param name="cancellationTokenSource">Source token used to cancel the worker's thread.</param>
        void Start(IGatewayService gateway, CancellationTokenSource cancellationTokenSource);

        /// <summary>
        /// Stop the gateway worker.
        /// </summary>
        void Stop();

        /// <summary>
        /// Emit bytes to the worker for processing.
        /// </summary>
        /// <param name="messageChunk">The message chunk to emit.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The created task.</returns>
        Task Emit(GatewayMessageChunk messageChunk, CancellationToken cancellationToken = default);
    }
}
