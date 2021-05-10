using System;

namespace Brighid.Discord.RestQueue.Requests
{
    /// <summary>
    /// Exception that is thrown when a message visibility timeout fails to update.
    /// </summary>
    public class VisibilityTimeoutNotUpdatedException : Exception, IRequestMessageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityTimeoutNotUpdatedException" /> class.
        /// </summary>
        /// <param name="message">The message that failed to update.</param>
        public VisibilityTimeoutNotUpdatedException(RequestMessage message)
        {
            RequestMessage = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityTimeoutNotUpdatedException" /> class.
        /// </summary>
        public VisibilityTimeoutNotUpdatedException()
        {
            RequestMessage = null!;
        }

        /// <inheritdoc />
        public override string Message => $"Request {RequestMessage?.RequestDetails?.Id}'s visibility timeout failed to update.";

        /// <inheritdoc />
        public RequestMessage RequestMessage { get; init; }
    }
}
