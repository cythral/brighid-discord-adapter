using Brighid.Discord.Models;

namespace Brighid.Discord.RestQueue.Requests
{
    /// <summary>
    /// Encapsulates a message from SQS and its metadata.
    /// </summary>
    public class RequestMessage
    {
        /// <summary>
        /// Gets or sets the request details.
        /// </summary>
        public Request Request { get; set; }

        /// <summary>
        /// Gets or sets the state of the request message.
        /// </summary>
        public RequestMessageState State { get; set; }
    }
}
