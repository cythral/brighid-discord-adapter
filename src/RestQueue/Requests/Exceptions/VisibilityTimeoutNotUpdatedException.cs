using System;

namespace Brighid.Discord.RestQueue.Requests
{
    /// <summary>
    /// Exception that is thrown when a message visibility timeout fails to update.
    /// </summary>
    public class VisibilityTimeoutNotUpdatedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityTimeoutNotUpdatedException" /> class.
        /// </summary>
        /// <param name="message">The message that failed to update.</param>
        public VisibilityTimeoutNotUpdatedException(RequestMessage message)
            : base($"Request {message.Request.Id}'s visibility timeout failed to update.")
        {
            RequestMessage = message;
        }

        /// <summary>
        /// Gets or sets the message that failed to update.
        /// </summary>
        public RequestMessage RequestMessage { get; set; }
    }
}
