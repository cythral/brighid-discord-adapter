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
                string userId,
                [Frozen] MockHttpMessageHandler handler,
                [Target] CacheService cache,
                CancellationToken cancellationToken
            )
            {
                var ip = IPAddress.Parse("9d89:15fe:df6f:6634:d1ed:6ae3:ef11:d0cb");

                handler
                .Expect(HttpMethod.Delete, $"http://[{ip}]/cache/users/{userId}")
                .Respond(HttpStatusCode.OK);

                await cache.ExpireUserCache(ip, userId, cancellationToken);

                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldThrowIfNonSuccessfulStatusCode(
                string userId,
                [Frozen] MockHttpMessageHandler handler,
                [Target] CacheService cache,
                CancellationToken cancellationToken
            )
            {
                var ip = IPAddress.Parse("b6e7:3d3b:b71c:0594:5012:49f6:e6d3:6270");

                handler
                .Expect(HttpMethod.Delete, $"http://[{ip}]/cache/users/{userId}")
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
                string userId,
                [Frozen] MockHttpMessageHandler handler,
                [Target] CacheService cache,
                CancellationToken cancellationToken
            )
            {
                var ip = IPAddress.Parse("b47e:925f:4be9:0c72:4a14:1290:0270:5a9b");

                handler
                .Expect(HttpMethod.Delete, $"http://[{ip}]/cache/commands/{userId}")
                .Respond(HttpStatusCode.OK);

                await cache.ExpireCommandCache(ip, userId, cancellationToken);

                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldThrowIfNonSuccessfulStatusCode(
                string userId,
                [Frozen] MockHttpMessageHandler handler,
                [Target] CacheService cache,
                CancellationToken cancellationToken
            )
            {
                var ip = IPAddress.Parse("c034:05ed:8e31:d24a:af63:b2bf:7d2b:7288");

                handler
                .Expect(HttpMethod.Delete, $"http://[{ip}]/cache/commands/{userId}")
                .Respond(HttpStatusCode.InternalServerError);

                Func<Task> func = () => cache.ExpireCommandCache(ip, userId, cancellationToken);

                await func.Should().ThrowAsync<Exception>();
                handler.VerifyNoOutstandingExpectation();
            }
        }
    }
}
