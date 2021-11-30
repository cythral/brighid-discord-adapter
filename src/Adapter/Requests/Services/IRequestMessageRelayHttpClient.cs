using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// HTTP Client for the <see iref="IRequestMessageRelay" /> service.
    /// </summary>
    public interface IRequestMessageRelayHttpClient
    {
        /// <summary>
        /// Posts a response back to the requestor.
        /// </summary>
        /// <param name="url">The url to respond to.</param>
        /// <param name="response">The response payload.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Post(Uri url, Response response, CancellationToken cancellationToken);
    }
}
