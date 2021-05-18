using System.Net.Http;
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

        /// <summary>
        /// Given an HTTP Response, add a bucket's remote ID, or merge this bucket with an old one.
        /// </summary>
        /// <param name="bucket">Bucket to update.</param>
        /// <param name="response">The HTTP Response to pull the bucket ID from.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting bucket.</returns>
        Task<Bucket> MergeBucketIds(Bucket bucket, HttpResponseMessage response, CancellationToken cancellationToken = default);

        /// <summary>
        /// Given an HTTP Response, update a bucket's rate limit reset after value.
        /// </summary>
        /// <param name="bucket">The bucket to update.</param>
        /// <param name="response">The HTTP Response to pull the reset after value from.</param>
        void UpdateBucketResetAfter(Bucket bucket, HttpResponseMessage response);

        /// <summary>
        /// Given an HTTP Response, update a bucket's hits remaining value.
        /// </summary>
        /// <param name="bucket">The bucket to update.</param>
        /// <param name="response">The HTTP Response to pull the hits remaining value from.</param>
        void UpdateBucketHitsRemaining(Bucket bucket, HttpResponseMessage response);
    }
}
