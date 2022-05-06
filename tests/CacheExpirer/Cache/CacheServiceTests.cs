using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using FluentAssertions;

using NUnit.Framework;

using RichardSzalay.MockHttp;

namespace Brighid.Discord.CacheExpirer
{
    public class CacheServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class ExpireUserCacheTests
        {
            [Test, Auto]
            public async Task ShouldSendADeleteRequestToTheClearUserCacheEndpoint(
                IPAddress ip,
                string userId,
                [Frozen] MockHttpMessageHandler handler,
                [Target] CacheService cache,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Delete, $"http://{ip}/cache/users/{userId}")
                .Respond(HttpStatusCode.OK);

                await cache.ExpireUserCache(ip, userId, cancellationToken);

                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldThrowIfNonSuccessfulStatusCode(
                IPAddress ip,
                string userId,
                [Frozen] MockHttpMessageHandler handler,
                [Target] CacheService cache,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Delete, $"http://{ip}/cache/users/{userId}")
                .Respond(HttpStatusCode.InternalServerError);

                Func<Task> func = () => cache.ExpireUserCache(ip, userId, cancellationToken);

                await func.Should().ThrowAsync<Exception>();
                handler.VerifyNoOutstandingExpectation();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class ExpireCommandCacheTests
        {
            [Test, Auto]
            public async Task ShouldSendADeleteRequestToTheClearCommandCacheEndpoint(
                IPAddress ip,
                string userId,
                [Frozen] MockHttpMessageHandler handler,
                [Target] CacheService cache,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Delete, $"http://{ip}/cache/commands/{userId}")
                .Respond(HttpStatusCode.OK);

                await cache.ExpireCommandCache(ip, userId, cancellationToken);

                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldThrowIfNonSuccessfulStatusCode(
                IPAddress ip,
                string userId,
                [Frozen] MockHttpMessageHandler handler,
                [Target] CacheService cache,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Delete, $"http://{ip}/cache/commands/{userId}")
                .Respond(HttpStatusCode.InternalServerError);

                Func<Task> func = () => cache.ExpireCommandCache(ip, userId, cancellationToken);

                await func.Should().ThrowAsync<Exception>();
                handler.VerifyNoOutstandingExpectation();
            }
        }
    }
}
