using System;
using System.Net.Http;
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
        [Category("Unit")]
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
            public async Task ShouldJitter(
                Request request,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                await service.GetBucketAndWaitForAvailability(request, cancellationToken);

                await timerFactory.Received().CreateJitter(Is(cancellationToken));
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
                var resetAfter = DateTimeOffset.Now;
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
            public async Task ShouldNotDelayIfHitsRemainingIsNotZeroOr1(
                Request request,
                [Frozen] Bucket bucket,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Frozen, Substitute] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                bucket.HitsRemaining = 2;
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

        [TestFixture]
        public class MergeBucketIdsTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                Bucket bucket,
                [Target] DefaultBucketService service
            )
            {
                var httpResponse = new HttpResponseMessage();
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => service.MergeBucketIds(bucket, httpResponse, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                Bucket bucket,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                var httpResponse = new HttpResponseMessage();

                Func<Task> func = () => service.MergeBucketIds(bucket, httpResponse, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfHeaderIsNotPresent(
                string remoteId,
                Bucket bucket,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                var httpResponse = new HttpResponseMessage();

                Func<Task> func = () => service.MergeBucketIds(bucket, httpResponse, cancellationToken);

                await func.Should().NotThrowAsync();
            }

            [Test, Auto]
            public async Task ShouldUpdateRemoteIdIfItAlreadyHasOne(
                string existingId,
                string newId,
                Bucket bucket,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                bucket.RemoteId = existingId;
                var httpResponse = new HttpResponseMessage();
                httpResponse.Headers.Add("x-ratelimit-bucket", new[] { newId });

                var result = await service.MergeBucketIds(bucket, httpResponse, cancellationToken);

                result.RemoteId.Should().Be(newId);
            }

            [Test, Auto]
            public async Task ShouldAddEndpointToExistingBucketIfOneExistsWithTheGivenRemoteId(
                string remoteId,
                Bucket bucket,
                Bucket existingBucket,
                [Frozen] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                bucket.RemoteId = null;
                bucket.Endpoints = (long)ChannelEndpoint.CreateMessage;
                bucket.ApiCategory = 'c';
                existingBucket.Endpoints = (long)ChannelEndpoint.DeleteMessage;
                existingBucket.ApiCategory = 'c';

                var httpResponse = new HttpResponseMessage();
                httpResponse.Headers.Add("x-ratelimit-bucket", new[] { remoteId });

                repository.FindByRemoteIdAndMajorParameters(Any<string>(), Any<MajorParameters>(), Any<CancellationToken>()).Returns(existingBucket);
                var result = await service.MergeBucketIds(bucket, httpResponse, cancellationToken);

                result.Should().Be(existingBucket);
                existingBucket.HasEndpoint(ChannelEndpoint.CreateMessage).Should().BeTrue();
                await repository.Received().FindByRemoteIdAndMajorParameters(Is(remoteId), Is(bucket.MajorParameters), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldRemoveNewBucketIfOneAlreadyExistsWithTheGivenRemoteId(
                string remoteId,
                Bucket bucket,
                Bucket existingBucket,
                [Frozen] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                bucket.RemoteId = null;
                bucket.Endpoints = (long)ChannelEndpoint.CreateMessage;
                bucket.ApiCategory = 'c';
                existingBucket.Endpoints = (long)ChannelEndpoint.DeleteMessage;
                existingBucket.ApiCategory = 'c';

                var httpResponse = new HttpResponseMessage();
                httpResponse.Headers.Add("x-ratelimit-bucket", new[] { remoteId });

                repository.FindByRemoteIdAndMajorParameters(Any<string>(), Any<MajorParameters>(), Any<CancellationToken>()).Returns(existingBucket);
                await service.MergeBucketIds(bucket, httpResponse, cancellationToken);

                await repository.Received().Remove(Is(bucket), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldUpdateTheNewBucketsRemoteIdIfNoExistingBucketWithRemoteId(
                string remoteId,
                Bucket bucket,
                Bucket existingBucket,
                [Frozen] IBucketRepository repository,
                [Target] DefaultBucketService service,
                CancellationToken cancellationToken
            )
            {
                bucket.RemoteId = null;
                bucket.Endpoints = (long)ChannelEndpoint.CreateMessage;
                bucket.ApiCategory = 'c';
                existingBucket.Endpoints = (long)ChannelEndpoint.DeleteMessage;
                existingBucket.ApiCategory = 'c';

                var httpResponse = new HttpResponseMessage();
                httpResponse.Headers.Add("x-ratelimit-bucket", new[] { remoteId });

                repository.FindByRemoteIdAndMajorParameters(Any<string>(), Any<MajorParameters>(), Any<CancellationToken>()).Returns((Bucket)null!);
                var result = await service.MergeBucketIds(bucket, httpResponse, cancellationToken);

                result.Should().Be(bucket);
                result.RemoteId.Should().Be(remoteId);
                await repository.Received().FindByRemoteIdAndMajorParameters(Is(remoteId), Is(bucket.MajorParameters), Is(cancellationToken));
            }
        }

        [TestFixture]
        public class UpdateBucketResetAfterTests
        {
            [Test, Auto]
            public void ShouldUpdateResetAfterFromHeaderValue(
                Bucket bucket,
                [Target] DefaultBucketService service
            )
            {
                var response = new HttpResponseMessage();
                response.Headers.Add("x-ratelimit-reset", new[] { "1621277586" });

                service.UpdateBucketResetAfter(bucket, response);

                bucket.ResetAfter.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1621277586));
            }

            [Test, Auto]
            public void ShouldNotThrowIfHeaderNotPresent(
                Bucket bucket,
                [Target] DefaultBucketService service
            )
            {
                var response = new HttpResponseMessage();

                Action func = () => service.UpdateBucketResetAfter(bucket, response);

                func.Should().NotThrow();
            }

            [Test, Auto]
            public void ShouldNotThrowIfHeaderValueIsNotANumber(
                Bucket bucket,
                [Target] DefaultBucketService service
            )
            {
                var response = new HttpResponseMessage();
                response.Headers.Add("x-ratelimit-reset", new[] { "asdf" });

                Action func = () => service.UpdateBucketResetAfter(bucket, response);

                func.Should().NotThrow();
            }
        }

        [TestFixture]
        public class UpdateBucketsHitsRemainingTests
        {
            [Test, Auto]
            public void ShouldUpdateHitsRemaining(
                Bucket bucket,
                [Target] DefaultBucketService service
            )
            {
                var response = new HttpResponseMessage();
                response.Headers.Add("x-ratelimit-remaining", new[] { "12" });

                service.UpdateBucketHitsRemaining(bucket, response);

                bucket.HitsRemaining.Should().Be(12);
            }

            [Test, Auto]
            public void ShouldNotThrowIfHeaderNotPresent(
                Bucket bucket,
                [Target] DefaultBucketService service
            )
            {
                var response = new HttpResponseMessage();

                Action func = () => service.UpdateBucketHitsRemaining(bucket, response);

                func.Should().NotThrow();
            }

            [Test, Auto]
            public void ShouldNotThrowIfHeaderValueIsNotANumber(
                Bucket bucket,
                [Target] DefaultBucketService service
            )
            {
                var response = new HttpResponseMessage();
                response.Headers.Add("x-ratelimit-remaining", new[] { "asdf" });

                Action func = () => service.UpdateBucketHitsRemaining(bucket, response);

                func.Should().NotThrow();
            }
        }
    }
}
