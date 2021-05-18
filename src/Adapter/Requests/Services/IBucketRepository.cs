using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Repository service for locating buckets.
    /// </summary>
    public interface IBucketRepository
    {
        /// <summary>
        /// Creates a new bucket.
        /// </summary>
        /// <param name="bucket">Bucket to create.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting bucket.</returns>
        Task<Bucket> Add(Bucket bucket, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes an existing bucket.
        /// </summary>
        /// <param name="bucket">Bucket to delete.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Remove(Bucket bucket, CancellationToken cancellationToken);

        /// <summary>
        /// Save a bucket in the repository.
        /// </summary>
        /// <param name="bucket">Bucket to save.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Save(Bucket bucket, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find a bucket by it's remote ID.
        /// </summary>
        /// <param name="remoteId">The remote ID to look for.</param>
        /// <param name="parameters">The major parameters to look for.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting bucket, or null if not found.</returns>
        Task<Bucket?> FindByRemoteIdAndMajorParameters(string remoteId, MajorParameters parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find a bucket by it's endpoint and major parameters.
        /// </summary>
        /// <param name="endpoint">The endpoint associated with the bucket.</param>
        /// <param name="parameters">The major parameters associated with the bucket.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting bucket, or null if not found.</returns>
        Task<Bucket?> FindByEndpointAndMajorParameters(Endpoint endpoint, MajorParameters parameters, CancellationToken cancellationToken = default);
    }
}
