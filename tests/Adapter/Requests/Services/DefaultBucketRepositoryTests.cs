using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;

using FluentAssertions;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

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
        public class BeginTransactionTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                [Target] DefaultBucketRepository repository
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => repository.BeginTransaction(cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => repository.BeginTransaction(cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldStartANewTransaction(
                IDbContextTransaction contextTransaction,
                [Frozen] DatabaseContext context,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                context.Database.BeginTransactionAsync(Any<CancellationToken>()).Returns(contextTransaction);

                var result = await repository.BeginTransaction(cancellationToken);
                var transactionResult = result.Should().BeOfType<MysqlBucketTransaction>().Which;

                transactionResult.ContextTransaction.Should().Be(contextTransaction);
                transactionResult.Context.Should().Be(context);

                await context.Database.Received().BeginTransactionAsync(Is(cancellationToken));
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

        [TestFixture]
        public class FindByEndpointAndMajorParametersTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                string remoteId,
                Endpoint endpoint,
                MajorParameters majorParameters,
                [Target] DefaultBucketRepository repository
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => repository.FindByEndpointAndMajorParameters(endpoint, majorParameters, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                string remoteId,
                Endpoint endpoint,
                MajorParameters majorParameters,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => repository.FindByEndpointAndMajorParameters(endpoint, majorParameters, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldQueryBucketsByRemoteId_CaseWhenEndpointValuesAreDifferent(
                char category,
                string remoteId1,
                string remoteId2,
                Bucket bucket1,
                Bucket bucket2,
                Endpoint endpoint1,
                Endpoint endpoint2,
                MajorParameters majorParameters,
                [Frozen] DatabaseContext context,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                var buckets = new[] { bucket1, bucket2 };
                bucket1.Endpoints = Convert.ToUInt64(endpoint1.Value);
                bucket2.Endpoints = Convert.ToUInt64(endpoint2.Value);
                bucket1.ApiCategory = endpoint1.Category;
                bucket2.ApiCategory = endpoint2.Category;
                bucket1.MajorParameters = majorParameters;
                bucket2.MajorParameters = majorParameters;

                var mockBuckets = buckets.AsQueryable().BuildMockDbSet();
                mockBuckets.AsQueryable().Returns(mockBuckets);
                context.Buckets = mockBuckets;

                var result = await repository.FindByEndpointAndMajorParameters(endpoint1, majorParameters, cancellationToken);

                result.Should().Be(bucket1);
            }

            [Test, Auto]
            public async Task ShouldQueryBucketsByRemoteId_CaseWhenMajorParametersAreDifferent(
                char category,
                string remoteId1,
                string remoteId2,
                Bucket bucket1,
                Bucket bucket2,
                Endpoint endpoint,
                MajorParameters majorParameters1,
                MajorParameters majorParameters2,
                [Frozen] DatabaseContext context,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                var buckets = new[] { bucket1, bucket2 };
                bucket1.Endpoints = Convert.ToUInt64(endpoint.Value);
                bucket2.Endpoints = Convert.ToUInt64(endpoint.Value);
                bucket1.ApiCategory = endpoint.Category;
                bucket2.ApiCategory = endpoint.Category;
                bucket1.MajorParameters = majorParameters1;
                bucket2.MajorParameters = majorParameters2;

                var mockBuckets = buckets.AsQueryable().BuildMockDbSet();
                mockBuckets.AsQueryable().Returns(mockBuckets);
                context.Buckets = mockBuckets;

                var result = await repository.FindByEndpointAndMajorParameters(endpoint, majorParameters2, cancellationToken);

                result.Should().Be(bucket2);
            }
        }
    }
}
