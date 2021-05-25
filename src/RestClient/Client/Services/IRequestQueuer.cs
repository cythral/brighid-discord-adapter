using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.RestClient.Client
{
    /// <summary>
    /// Service used to queue requests.
    /// </summary>
    public interface IRequestQueuer
    {
        /// <summary>
        /// Queues a request to be invoked.
        /// </summary>
        /// <param name="request">The request to be invoked.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task QueueRequest(Request request, CancellationToken cancellationToken = default);
    }
}
