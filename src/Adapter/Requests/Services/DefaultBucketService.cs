using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.Threading;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultBucketService : IBucketService
    {
        private readonly IBucketRepository repository;
        private readonly ITimerFactory timerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBucketService" /> class.
        /// </summary>
        /// <param name="repository">Repository to search for buckets in.</param>
        /// <param name="timerFactory">Factory to create timers and delays with.</param>
        public DefaultBucketService(
            IBucketRepository repository,
            ITimerFactory timerFactory
        )
        {
            this.repository = repository;
            this.timerFactory = timerFactory;
        }

        /// <inheritdoc />
        /// <todo>Transaction handling / row read locking.</todo>
        public async Task<Bucket> GetBucketAndWaitForAvailability(Request request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await timerFactory.CreateJitter(cancellationToken);

            var bucket = await repository.FindByEndpointAndMajorParameters(request.Endpoint, request.Parameters, cancellationToken);
            bucket ??= await CreateBucket(request, cancellationToken);

            if (bucket.HitsRemaining <= 1)
            {
                var delay = (int)Math.Ceiling((bucket.ResetAfter - DateTimeOffset.Now).TotalMilliseconds);
                delay = Math.Max(delay, 100);
                await timerFactory.CreateDelay(delay, cancellationToken);
                bucket.HitsRemaining++;
            }

            bucket.HitsRemaining--;
            await repository.Save(bucket, cancellationToken);
            return bucket;
        }

        private async Task<Bucket> CreateBucket(Request request, CancellationToken cancellationToken = default)
        {
            var bucket = new Bucket
            {
                MajorParameters = request.Parameters,
                HitsRemaining = 1,
                ResetAfter = DateTimeOffset.Now + TimeSpan.FromMinutes(1),
            };

            bucket.AddEndpoint(request.Endpoint);
            return await repository.Add(bucket, cancellationToken);
        }
    }
}
