using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Database;

using FluentAssertions;

using Microsoft.EntityFrameworkCore.ChangeTracking;

using MockQueryable.NSubstitute;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Requests
{
    public class DefaultBucketRepositoryTests
    {
        [TestFixture]
        public class AddTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                Bucket bucket,
                [Target] DefaultBucketRepository repository
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => repository.Add(bucket, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                Bucket bucket,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => repository.Add(bucket, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldCreateANewBucket(
                Bucket bucket,
                Bucket resultingBucket,
                [Substitute] EntityEntry<Bucket> resultingBucketEntry,
                [Frozen] DatabaseContext databaseContext,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                resultingBucketEntry.Entity.Returns(resultingBucket);
                databaseContext.AddAsync(Any<Bucket>(), Any<CancellationToken>()).Returns(resultingBucketEntry);

                var result = await repository.Add(bucket, cancellationToken);

                result.Should().Be(resultingBucket);
                Received.InOrder(async () =>
                {
                    await databaseContext.Received().AddAsync(Is(bucket), Is(cancellationToken));
                    await databaseContext.Received().SaveChangesAsync(Is(cancellationToken));
                });
            }
        }

        [TestFixture]
        public class SaveTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                Bucket bucket,
                [Target] DefaultBucketRepository repository
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => repository.Save(bucket, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                Bucket bucket,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => repository.Save(bucket, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldStartANewTransaction(
                Bucket bucket,
                [Frozen] DatabaseContext context,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                await repository.Save(bucket, cancellationToken);

                context.Received().Attach(Is(bucket));
                await context.Received().SaveChangesAsync(Is(cancellationToken));
            }
        }

        [TestFixture]
        public class FindByRemoteIdTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                string remoteId,
                [Target] DefaultBucketRepository repository
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => repository.FindByRemoteId(remoteId, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                string remoteId,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => repository.FindByRemoteId(remoteId, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldQueryBucketsByRemoteId(
                string remoteId1,
                string remoteId2,
                Bucket bucket1,
                Bucket bucket2,
                [Frozen] DatabaseContext context,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                var buckets = new[] { bucket1, bucket2 };
                bucket1.RemoteId = remoteId1;
                bucket2.RemoteId = remoteId2;

                var mockBuckets = buckets.AsQueryable().BuildMockDbSet();
                mockBuckets.AsQueryable().Returns(mockBuckets);
                context.Buckets = mockBuckets;

                var result = await repository.FindByRemoteId(remoteId2, cancellationToken);

                result.Should().Be(bucket2);
            }
        }
    }
}
