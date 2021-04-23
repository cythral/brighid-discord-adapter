using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Identity.Client;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Users
{
    public class DefaultUserServiceTests
    {
        [TestFixture]
        public class IsUserRegisteredTests
        {
            [Test, Auto]
            public async Task ShouldReturnTrueIfUserIdExistsInCache(
                ulong userId,
                [Frozen, Substitute] IUserIdCache cache,
                [Target] DefaultUserService service
            )
            {
                var user = new Models.User { Id = userId };
                var cancellationToken = new CancellationToken(false);

                cache.ContainsKey(Any<Snowflake>()).Returns(true);

                var result = await service.IsUserRegistered(user, cancellationToken);

                result.Should().BeTrue();
                cache.Received().ContainsKey(Is(user.Id));
            }

            [Test, Auto]
            public async Task ShouldReturnTrueIfUserIdNotInCacheButReturnsFromTheApi(
                ulong userId,
                Guid remoteId,
                [Frozen, Substitute] IUserIdCache cache,
                [Frozen, Substitute] ILoginProvidersClient client,
                [Target] DefaultUserService service
            )
            {
                var user = new Models.User { Id = userId };
                var remoteUser = new Identity.Client.User { Id = remoteId };
                var cancellationToken = new CancellationToken(false);

                cache.ContainsKey(Any<Snowflake>()).Returns(false);
                client.GetUserByLoginProviderKey(Any<string>(), Any<string>(), Any<CancellationToken>()).Returns(remoteUser);

                var result = await service.IsUserRegistered(user, cancellationToken);

                result.Should().BeTrue();

                await client.Received().GetUserByLoginProviderKey(Is("discord"), Is(userId.ToString()), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCacheReturnedUserIds(
                ulong userId,
                Guid remoteId,
                [Frozen, Substitute] IUserIdCache cache,
                [Frozen, Substitute] ILoginProvidersClient client,
                [Target] DefaultUserService service
            )
            {
                var user = new Models.User { Id = userId };
                var remoteUser = new Identity.Client.User { Id = remoteId };
                var cancellationToken = new CancellationToken(false);

                cache.ContainsKey(Any<Snowflake>()).Returns(false);
                client.GetUserByLoginProviderKey(Any<string>(), Any<string>(), Any<CancellationToken>()).Returns(remoteUser);

                await service.IsUserRegistered(user, cancellationToken);

                cache.Received().Add(Is(user.Id), Is(remoteId));
            }

            [Test, Auto]
            public async Task ShouldReturnFalseIfLoginProvidersClientThrows(
                ulong userId,
                Guid remoteId,
                string errorMessage,
                ApiException exception,
                [Frozen, Substitute] IUserIdCache cache,
                [Frozen, Substitute] ILoginProvidersClient client,
                [Target] DefaultUserService service
            )
            {
                var user = new Models.User { Id = userId };
                var remoteUser = new Identity.Client.User { Id = remoteId };
                var cancellationToken = new CancellationToken(false);

                cache.ContainsKey(Any<Snowflake>()).Returns(false);
                client.GetUserByLoginProviderKey(Any<string>(), Any<string>(), Any<CancellationToken>()).Returns<Identity.Client.User>(x =>
                {
                    throw exception;
                });

                var result = await service.IsUserRegistered(user, cancellationToken);
                result.Should().BeFalse();

                await client.Received().GetUserByLoginProviderKey(Is("discord"), Is(userId.ToString()), Is(cancellationToken));
            }
        }
    }
}
