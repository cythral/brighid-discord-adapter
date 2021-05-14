using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.Threading;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Requests
{
    public class DefaultBucketServiceTests
    {
        [TestFixture]
        public class GetBucketAndWaitForAvailabilityTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                Request request,
                [Target] DefaultBucketService service
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => service.GetBucketAndWaitForAvailability(request, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                Request request,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => service.GetBucketAndWaitForAvailability(request, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldFetchBucketFromTheRepository(
                Request request,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                await repository.Received().FindByEndpointAndMajorParameters(Is(request.Endpoint), Is((MajorParameters)request.Parameters), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCreateNewBucketIfOneWasntFound(
                Request request,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                var resetAfter = DateTimeOffset.Now + TimeSpan.FromMinutes(1);
                repository.FindByEndpointAndMajorParameters(Any<Endpoint>(), Any<MajorParameters>(), Any<CancellationToken>()).Returns((Bucket?)null);
                await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                await repository.Received().Add(
                    Is<Bucket>(bucket =>
                        bucket.Endpoints == Convert.ToUInt64(request.Endpoint.Value) &&
                        bucket.ApiCategory == request.Endpoint.Category &&
                        bucket.MajorParameters == request.Parameters &&
                        bucket.HitsRemaining == 1 &&
                        bucket.ResetAfter >= resetAfter
                    ),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldNotCreateNewBucketIfOneWasFound(
                Request request,
                [Frozen] Bucket bucket,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                await repository.DidNotReceive().Add(
                    Is<Bucket>(bucket =>
                        bucket.Endpoints == Convert.ToUInt64(request.Endpoint.Value) &&
                        bucket.ApiCategory == request.Endpoint.Category &&
                        bucket.MajorParameters == request.Parameters &&
                        bucket.HitsRemaining == 1
                    ),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldCreateATransaction(
                Request request,
                [Frozen] Bucket bucket,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                await repository.Received().BeginTransaction(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldLockTheBucket(
                Request request,
                [Frozen] Bucket bucket,
                [Frozen, Substitute] IBucketTransaction transaction,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                await transaction.Received().LockBucket(Is(bucket), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCommitTheTransactionAfterSavingTheBucket(
                Request request,
                [Frozen] Bucket bucket,
                [Frozen, Substitute] IBucketTransaction transaction,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                Received.InOrder(async () =>
                {
                    await repository.Received().Save(Is(bucket), Is(cancellationToken));
                    await transaction.Received().Commit(Is(cancellationToken));
                });
            }

            [Test, Auto]
            public async Task ShouldDelayUntilResetAfterIsReachedIfHitsRemainingIsZero(
                int secondsToWait,
                Request request,
                [Frozen] Bucket bucket,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                bucket.HitsRemaining = 0;
                bucket.ResetAfter = DateTimeOffset.Now + TimeSpan.FromSeconds(secondsToWait);
                await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                await timerFactory.Received().CreateDelay(Is<int>(ms => ms >= (secondsToWait * 1000) - 20), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotDelayIfHitsRemainingIsNotZero(
                Request request,
                [Frozen] Bucket bucket,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                bucket.HitsRemaining = 1;
                await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                await timerFactory.DidNotReceive().CreateDelay(Is((int)Math.Ceiling((bucket.ResetAfter - DateTimeOffset.Now).TotalMilliseconds)), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldDecreaseTheNumberOfHitsByOne(
                Request request,
                [Frozen] Bucket bucket,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                bucket.HitsRemaining = 2;
                var result = await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                result.HitsRemaining.Should().Be(1);
                await repository.Received().Save(Is<Bucket>(recv => recv == bucket && recv.HitsRemaining == 1), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotDecreaseTheNumberOfHitsByOneIfHitsRemainingIsZero(
                Request request,
                [Frozen] Bucket bucket,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                bucket.HitsRemaining = 0;
                var result = await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                result.HitsRemaining.Should().Be(0);
                await repository.Received().Save(Is<Bucket>(recv => recv == bucket && recv.HitsRemaining == 0), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldReturnTheBucket(
                Request request,
                [Frozen] Bucket bucket,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                var result = await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                result.Should().Be(bucket);
            }
        }
    }
}
