using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Threading;

namespace Brighid.Discord.GatewayAdapter.Gateway
{
    /// <summary>
    /// Factory to create and supply various utilities that gateway services use.
    /// </summary>
    public interface IGatewayUtilsFactory
    {
        /// <summary>
        /// Create a new websocket client.
        /// </summary>
        /// <returns>The resulting websocket client.</returns>
        IClientWebSocket CreateWebSocketClient();

        /// <summary>
        /// Create a new worker thread.
        /// </summary>
        /// <param name="runAsync">The thread's async run method.</param>
        /// <param name="workerName">The name of the worker.</param>
        /// <returns>The resulting worker thread.</returns>
        IWorkerThread CreateWorkerThread(Func<Task> runAsync, string workerName);

        /// <summary>
        /// Create a new channel.
        /// </summary>
        /// <typeparam name="TMessage">The type of message that will go through the channel.</typeparam>
        /// <returns>The resulting channel.</returns>
        IChannel<TMessage> CreateChannel<TMessage>();

        /// <summary>
        /// Creates a new memory stream.
        /// </summary>
        /// <returns>The resulting stream.</returns>
        Stream CreateStream();

        /// <summary>
        /// Creates a task that completes after a delay.
        /// </summary>
        /// <param name="millisecondsDelay">Milliseconds to delay for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task CreateDelay(uint millisecondsDelay, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a task that completes after a delay between <paramref name="minimum" /> milliseconds and <paramref name="maximum" /> milliseconds.
        /// </summary>
        /// <param name="minimum">Minimum milliseconds to delay for.</param>
        /// <param name="maximum">Maximum milliseconds to delay for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task CreateRandomDelay(uint minimum, uint maximum, CancellationToken cancellationToken);
    }
}
