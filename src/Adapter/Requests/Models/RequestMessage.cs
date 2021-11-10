using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Encapsulates a message from SQS and its metadata.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class RequestMessage
    {
        /// <summary>
        /// Gets or sets the request details.
        /// </summary>
        public Request RequestDetails { get; set; } = null!;

        /// <summary>
        /// Gets or sets the message receipt handle.
        /// </summary>
        public string ReceiptHandle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the token used to cancel processing the request.
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = new CancellationToken(false);

        /// <summary>
        /// Gets or sets the promise task that will complete when this task has finished processing.
        /// </summary>
        public TaskCompletionSource Promise { get; set; } = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Gets or sets the Request Message's response code.
        /// </summary>
        public HttpStatusCode ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the Request Message's response to send back to the requester when complete.
        /// </summary>
        public string? Response { get; set; }

        /// <summary>
        /// Gets or sets the new visibility timeout to use if the message fails processing.
        /// </summary>
        public uint VisibilityTimeout { get; set; }

        /// <summary>
        /// Gets or sets the state of the request message.
        /// </summary>
        public RequestMessageState State { get; set; }
    }
}
