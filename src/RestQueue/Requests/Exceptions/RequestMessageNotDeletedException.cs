using System;

namespace Brighid.Discord.RestQueue.Requests
{
    /// <summary>
    /// Exception that is thrown when a message fails to delete.
    /// </summary>
    public class RequestMessageNotDeletedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessageNotDeletedException" /> class.
        /// </summary>
        /// <param name="message">The request message that failed to delete.</param>
        public RequestMessageNotDeletedException(RequestMessage message)
            : base($"Request {message.RequestDetails.Id} failed to delete in the queue.")
        {
            RequestMessage = message;
        }

        /// <summary>
        /// Gets or sets the message that failed to delete.
        /// </summary>
        public RequestMessage RequestMessage { get; set; }
    }
}
