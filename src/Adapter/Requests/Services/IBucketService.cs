using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Service to handle Rate Limit Buckets.
    /// </summary>
    public interface IBucketService
    {
        /// <summary>
        /// Waits for bucket availability.
        /// </summary>
        /// <param name="request">The request to get a bucket for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task that will complete when the bucket becomes available again.</returns>
        Task<Bucket> GetBucketAndWaitForAvailability(Request request, CancellationToken cancellationToken = default);
    }
}
