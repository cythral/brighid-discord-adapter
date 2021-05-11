using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Service to relay messages to/from a queue.
    /// </summary>
    public interface IRequestMessageRelay
    {
        /// <summary>
        /// Receives messages from the queue to begin processing it.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting messages.</returns>
        Task<IEnumerable<RequestMessage>> Receive(CancellationToken cancellationToken = default);

        /// <summary>
        /// Complete a message by deleting it from the queue.
        /// </summary>
        /// <param name="message">Message to complete.</param>
        /// <param name="statusCode">Status code to send back to the requester.</param>
        /// <param name="response">Response to send to the requester, or null if no response should be sent.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Complete(RequestMessage message, HttpStatusCode statusCode, string? response = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Signals that a message failed to process.  The message will be marked as visible to other workers after <paramref name="visibilityTimeout" /> seconds.
        /// </summary>
        /// <param name="message">Message to mark as failed to process.</param>
        /// <param name="visibilityTimeout">Number of seconds to wait before the message will become available for processing again.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Fail(RequestMessage message, uint visibilityTimeout = 0, CancellationToken cancellationToken = default);
    }
}
