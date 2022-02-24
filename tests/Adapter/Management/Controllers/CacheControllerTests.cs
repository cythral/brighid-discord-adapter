using System;
using System.Net;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Commands.Client;
using Brighid.Discord.Adapter.Users;
using Brighid.Identity.Client.Utils;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Management
{
    public class CacheControllerTests
    {
        [TestFixture]
        [Category("Unit")]
        public class ClearAllTests
        {
            [Test, Auto]
            public void ShouldClearTheUserIdCache(
                [Frozen, Substitute] IUserIdCache userIdCache,
                [Target] CacheController controller
            )
            {
                controller.ClearAll();

                userIdCache.Received().Clear();
            }

            [Test, Auto]
            public void ShouldInvalidateTheIdentityToken(
                [Frozen, Substitute] ICacheUtils cacheUtils,
                [Target] CacheController controller
            )
            {
                controller.ClearAll();

                cacheUtils.Received().InvalidatePrimaryToken();
            }

            [Test, Auto]
            public void ShouldInvalidateAllUserTokens(
                [Frozen, Substitute] ICacheUtils cacheUtils,
                [Target] CacheController controller
            )
            {
                controller.ClearAll();

                cacheUtils.Received().InvalidateAllUserTokens();
            }

            [Test, Auto]
            public void ShouldClearTheCommandsCache(
                [Frozen, Substitute] IBrighidCommandsCache commandsCache,
                [Target] CacheController controller
            )
            {
                controller.ClearAll();

                commandsCache.Received().ClearAllParameters();
            }

            [Test, Auto]
            public void ShouldReturnOk(
                [Frozen, Substitute] ICacheUtils cacheUtils,
                [Target] CacheController controller
            )
            {
                var result = controller.ClearAll();

                result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class ClearCommandSpecificCacheTests
        {
            [Test, Auto]
            public void ShouldClearCommandParameters(
                string name,
                [Frozen, Substitute] IBrighidCommandsCache commandsCache,
                [Target] CacheController controller
            )
            {
                controller.ClearCommandSpecificCache(name);

                commandsCache.Received().ClearParameters(Is(name));
            }

            [Test, Auto]
            public void ShouldReturnOk(
                string name,
                [Target] CacheController controller
            )
            {
                var result = controller.ClearCommandSpecificCache(name);

                result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class ClearUserSpecificCacheTests
        {
            [Test, Auto]
            public void ShouldRemoveTheUserIdCacheEntryForTheGivenIdentityId(
                Guid identityId,
                [Frozen, Substitute] IUserIdCache userIdCache,
                [Target] CacheController controller
            )
            {
                controller.ClearUserSpecificCache(identityId);

                userIdCache.Received().RemoveByIdentityId(Is(identityId));
            }

            [Test, Auto]
            public void ShouldRemoveAnyUserTokensForTheGivenIdentityId(
                Guid identityId,
                [Frozen, Substitute] ICacheUtils cacheUtils,
                [Target] CacheController controller
            )
            {
                controller.ClearUserSpecificCache(identityId);

                cacheUtils.Received().InvalidateTokensForUser(Is(identityId.ToString()));
            }

            [Test, Auto]
            public void ShouldReturnOk(
                Guid identityId,
                [Target] CacheController controller
            )
            {
                var result = controller.ClearUserSpecificCache(identityId);

                result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            }
        }
    }
}
