using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;

using Microsoft.EntityFrameworkCore;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultBucketRepository : IBucketRepository
    {
        private readonly DatabaseContext databaseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBucketRepository" /> class.
        /// </summary>
        /// <param name="databaseContext">Database context to use.</param>
        public DefaultBucketRepository(
            DatabaseContext databaseContext
        )
        {
            this.databaseContext = databaseContext;
        }

        /// <inheritdoc />
        public async Task<Bucket> Add(Bucket bucket, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = await databaseContext.AddAsync(bucket, cancellationToken);
            await databaseContext.SaveChangesAsync(cancellationToken);
            return entry.Entity;
        }

        /// <inheritdoc />
        public async Task<IBucketTransaction> BeginTransaction(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var contextTransaction = await databaseContext.Database.BeginTransactionAsync(cancellationToken);
            return new MysqlBucketTransaction(contextTransaction, databaseContext);
        }

        /// <inheritdoc />
        public async Task Save(Bucket bucket, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            databaseContext.Attach(bucket);
            await databaseContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Bucket?> FindByRemoteId(string remoteId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = from bucket in databaseContext.Buckets.AsQueryable()
                        where bucket.RemoteId == remoteId
                        select bucket;

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Bucket?> FindByEndpointAndMajorParameters(Endpoint endpoint, MajorParameters parameters, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = from bucket in databaseContext.Buckets.AsQueryable()
                        let endpointValue = Convert.ToUInt64(endpoint.Value)
                        where bucket.ApiCategory == endpoint.Category
                            && (bucket.Endpoints & endpointValue) == endpointValue
                            && bucket.MajorParameters == parameters
                        select bucket;

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
    }
}
