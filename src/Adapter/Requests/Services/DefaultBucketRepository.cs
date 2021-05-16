using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Database;
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
            var endpointValue = Convert.ToUInt64(endpoint.Value);
            var query = databaseContext.Buckets.FromSqlInterpolated(
                $@"select * from Buckets 
                    where ApiCategory={endpoint.Category}
                    and (Endpoints & {endpointValue}) = {endpointValue}
                    and MajorParameters = {parameters.Value}
                    limit 1
                    for update
                "
            );

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
    }
}
