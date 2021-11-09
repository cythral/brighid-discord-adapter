using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Database;

using FluentAssertions;

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
        }

        [TestFixture]
        public class RemoveTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                Bucket bucket,
                [Target] DefaultBucketRepository repository
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => repository.Remove(bucket, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                Bucket bucket,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => repository.Remove(bucket, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldRemoveTheBucketFromTheDatabase(
                Bucket bucket,
                [Frozen] DatabaseContext databaseContext,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                await repository.Remove(bucket, cancellationToken);

                databaseContext.Received().Remove(Is(bucket));
                await databaseContext.Received().SaveChangesAsync(Is(cancellationToken));
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
            public async Task ShouldSaveChanges(
                Bucket bucket,
                [Frozen] DatabaseContext context,
                [Target] DefaultBucketRepository repository,
                CancellationToken cancellationToken
            )
            {
                await repository.Save(bucket, cancellationToken);

                await context.Received().SaveChangesAsync(Is(cancellationToken));
            }
        }
    }
}
