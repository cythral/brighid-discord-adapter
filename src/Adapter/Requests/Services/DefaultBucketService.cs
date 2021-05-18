using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Discord.Threading;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultBucketService : IBucketService
    {
        private const string RateLimitResetHeader = "x-ratelimit-reset";
        private const string RateLimitRemainingHeader = "x-ratelimit-remaining";
        private const string RateLimitBucketHeader = "x-ratelimit-bucket";
        private readonly IBucketRepository repository;
        private readonly ITimerFactory timerFactory;
        private readonly ILogger<DefaultBucketService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBucketService" /> class.
        /// </summary>
        /// <param name="repository">Repository to search for buckets in.</param>
        /// <param name="timerFactory">Factory to create timers and delays with.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultBucketService(
            IBucketRepository repository,
            ITimerFactory timerFactory,
            ILogger<DefaultBucketService> logger
        )
        {
            this.repository = repository;
            this.timerFactory = timerFactory;
            this.logger = logger;
        }

        /// <inheritdoc />
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
            return bucket;
        }

        /// <inheritdoc />
        public async Task<Bucket> MergeBucketIds(Bucket bucket, HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!response.Headers.TryGetValues(RateLimitBucketHeader, out var headerValues))
            {
                logger.LogWarning($"Response did not have {RateLimitBucketHeader} header.");
                return bucket;
            }

            var remoteId = headerValues.First();
            if (bucket.RemoteId != null)
            {
                bucket.RemoteId = remoteId;
                return bucket;
            }

            var existingBucket = await repository.FindByRemoteId(remoteId, cancellationToken);
            if (existingBucket == null)
            {
                bucket.RemoteId = remoteId;
                return bucket;
            }

            repository.Remove(bucket);
            existingBucket.Endpoints |= bucket.Endpoints;
            return existingBucket;
        }

        /// <inheritdoc />
        public void UpdateBucketResetAfter(Bucket bucket, HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues(RateLimitResetHeader, out var values))
            {
                logger.LogWarning($"Response did not have {RateLimitResetHeader} header.");
                return;
            }

            if (!long.TryParse(values.First(), out var resetAfterSeconds))
            {
                logger.LogWarning($"Response had a non-numeric {RateLimitResetHeader} header value.");
                return;
            }

            bucket.ResetAfter = DateTimeOffset.FromUnixTimeSeconds(resetAfterSeconds);
        }

        /// <inheritdoc />
        public void UpdateBucketHitsRemaining(Bucket bucket, HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues(RateLimitRemainingHeader, out var headerValues))
            {
                logger.LogWarning($"Response did not have {RateLimitRemainingHeader} header.");
                return;
            }

            if (!int.TryParse(headerValues.First(), out var hitsRemaining))
            {
                logger.LogWarning($"Response had a non-numeric {hitsRemaining} header value.");
                return;
            }

            bucket.HitsRemaining = hitsRemaining;
        }

        private async Task<Bucket> CreateBucket(Request request, CancellationToken cancellationToken = default)
        {
            var bucket = new Bucket
            {
                MajorParameters = request.Parameters,
                HitsRemaining = 1,
                ResetAfter = DateTimeOffset.Now + TimeSpan.FromMinutes(0.1),
            };

            bucket.AddEndpoint(request.Endpoint);
            return await repository.Add(bucket, cancellationToken);
        }
    }
}
