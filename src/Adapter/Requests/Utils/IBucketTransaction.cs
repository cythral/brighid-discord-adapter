using System;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Requests
{
    /// <summary>
    /// Service for transacting buckets.
    /// </summary>
    public interface IBucketTransaction : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Locks the bucket so it cannot be used until this transaction is over.
        /// </summary>
        /// <param name="bucket">Bucket to lock.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The locked bucket with reloaded data.</returns>
        Task LockBucket(Bucket bucket, CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the transaction and releases the bucket lock.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Commit(CancellationToken cancellationToken = default);
    }
}
