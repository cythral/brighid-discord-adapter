using System;
using System.Net;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Users;
using Brighid.Identity.Client;

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
                [Frozen, Substitute] ITokenStore tokenStore,
                [Target] CacheController controller
            )
            {
                controller.ClearAll();

                tokenStore.Received().InvalidateToken();
            }

            [Test, Auto]
            public void ShouldInvalidateAllUserTokens(
                [Frozen, Substitute] IUserTokenStore userTokenStore,
                [Target] CacheController controller
            )
            {
                controller.ClearAll();

                userTokenStore.Received().InvalidateAllUserTokens();
            }

            [Test, Auto]
            public void ShouldReturnOk(
                [Frozen, Substitute] IUserTokenStore userTokenStore,
                [Target] CacheController controller
            )
            {
                var result = controller.ClearAll();

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

                userIdCache.Received().RemoveByIdentityId(identityId);
            }

            [Test, Auto]
            public void ShouldRemoveAnyUserTokensForTheGivenIdentityId(
                Guid identityId,
                [Frozen, Substitute] IUserTokenStore userTokenStore,
                [Target] CacheController controller
            )
            {
                controller.ClearUserSpecificCache(identityId);

                userTokenStore.Received().InvalidateTokensForUser(Is(identityId.ToString()));
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
