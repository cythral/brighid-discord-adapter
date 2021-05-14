using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Storage;

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class MysqlBucketTransaction : IBucketTransaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MysqlBucketTransaction" /> class.
        /// </summary>
        /// <param name="dbContextTransaction">Source transaction handle.</param>
        /// <param name="context">Context to use for database interactions.</param>
        public MysqlBucketTransaction(
            IDbContextTransaction dbContextTransaction,
            DatabaseContext context
        )
        {
            ContextTransaction = dbContextTransaction;
            Context = context;
        }

        /// <summary>
        /// Gets the transaction from the database context.
        /// </summary>
        public IDbContextTransaction ContextTransaction { get; }

        /// <summary>
        /// Gets the database context this transaction belongs to.
        /// </summary>
        public DatabaseContext Context { get; }

        /// <inheritdoc />
        public async Task LockBucket(Bucket bucket, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Context.ExecuteSqlInterpolated($"select * from Buckets where RemoteId={bucket.RemoteId} for update", cancellationToken);
            await Context.ReloadEntity(bucket, cancellationToken);
        }

        /// <inheritdoc />
        public async Task Commit(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ContextTransaction.CommitAsync(cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return ContextTransaction.DisposeAsync();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ContextTransaction.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
