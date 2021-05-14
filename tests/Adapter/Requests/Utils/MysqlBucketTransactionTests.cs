using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.EntityFrameworkCore.Storage;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Requests
{
    [TestFixture]
    public class MysqlBucketTransactionTests
    {
        [TestFixture]
        public class LockBucketTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                Bucket bucket,
                [Target] MysqlBucketTransaction transaction
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => transaction.LockBucket(bucket, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                Bucket bucket,
                [Target] MysqlBucketTransaction transaction,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => transaction.LockBucket(bucket, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldLockBucketViaSelectForUpdate(
                Bucket bucket,
                [Frozen] DatabaseContext databaseContext,
                [Target] MysqlBucketTransaction transaction,
                CancellationToken cancellationToken
            )
            {
                await transaction.LockBucket(bucket, cancellationToken);

                await databaseContext.Received().ExecuteSqlInterpolated(Is<FormattableString>(recv => (string?)recv.GetArgument(0) == bucket.RemoteId && recv.Format == "select * from Buckets where RemoteId={0} for update"), Is(cancellationToken));
            }
        }

        [TestFixture]
        public class CommitTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                [Target] MysqlBucketTransaction transaction
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => transaction.Commit(cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                [Target] MysqlBucketTransaction transaction,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => transaction.Commit(cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldCommitTheTransaction(
                Bucket bucket,
                [Frozen] IDbContextTransaction contextTransaction,
                [Target] MysqlBucketTransaction transaction,
                CancellationToken cancellationToken
            )
            {
                await transaction.Commit(cancellationToken);

                await contextTransaction.Received().CommitAsync(Is(cancellationToken));
            }
        }
    }
}
