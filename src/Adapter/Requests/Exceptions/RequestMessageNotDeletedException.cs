using System;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Exception that is thrown when a message fails to delete.
    /// </summary>
    public class RequestMessageNotDeletedException : Exception, IRequestMessageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessageNotDeletedException" /> class.
        /// </summary>
        /// <param name="message">The request message that failed to delete.</param>
        public RequestMessageNotDeletedException(RequestMessage message)
        {
            RequestMessage = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessageNotDeletedException" /> class.
        /// </summary>
        public RequestMessageNotDeletedException()
        {
            RequestMessage = null!;
        }

        /// <inheritdoc />
        public override string Message => $"Request {RequestMessage?.RequestDetails?.Id} failed to delete in the queue.";

        /// <inheritdoc />
        public RequestMessage RequestMessage { get; init; }
    }
}
