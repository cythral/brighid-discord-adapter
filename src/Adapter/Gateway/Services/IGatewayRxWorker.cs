using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Messages;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <summary>
    /// A worker to receive bytes from a buffer and parse them into a message.
    /// </summary>
    public interface IGatewayRxWorker
    {
        /// <summary>
        /// Gets a value indicating whether the service is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets a list of currently running or waiting tasks.
        /// </summary>
        Dictionary<int, Task> TaskQueue { get; }

        /// <summary>
        /// Start the worker.
        /// </summary>
        /// <param name="gateway">The gateway to work on.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Start(IGatewayService gateway, CancellationToken cancellationToken);

        /// <summary>
        /// Stop the gateway worker.
        /// </summary>
        /// <returns>The resulting task.</returns>
        Task Stop();

        /// <summary>
        /// Emit bytes to the worker for processing.
        /// </summary>
        /// <param name="messageChunk">The message chunk to emit.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The created task.</returns>
        Task Emit(GatewayMessageChunk messageChunk, CancellationToken cancellationToken = default);
    }
}
