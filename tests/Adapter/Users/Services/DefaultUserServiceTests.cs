using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Management;
using Brighid.Discord.Models;
using Brighid.Identity.Client;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using RichardSzalay.MockHttp;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Users
{
    public class DefaultUserServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class ExchangeOAuth2CodeForTokenTests
        {
            [Test, Auto]
            public async Task ShouldMakeARequestToDiscordsTokenEndpointWithTheCode(
                string code,
                string accessToken,
                [Frozen] IDiscordApiInfoService discordApiInfo,
                [Frozen] MockHttpMessageHandler handler,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Post, discordApiInfo.OAuth2TokenEndpoint.ToString())
                .WithFormData("code", code)
                .Respond("application/json", @$"{{""access_token"":""{accessToken}""}}");

                await service.ExchangeOAuth2CodeForToken(code, cancellationToken);

                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldMakeARequestToDiscordsTokenEndpointWithTheClientId(
                string code,
                string accessToken,
                [Frozen] IDiscordApiInfoService discordApiInfo,
                [Frozen] MockHttpMessageHandler handler,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Post, discordApiInfo.OAuth2TokenEndpoint.ToString())
                .WithFormData("client_id", discordApiInfo.ClientId)
                .Respond("application/json", @$"{{""access_token"":""{accessToken}""}}");

                await service.ExchangeOAuth2CodeForToken(code, cancellationToken);

                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldMakeARequestToDiscordsTokenEndpointWithTheClientSecret(
                string code,
                string accessToken,
                [Frozen] IDiscordApiInfoService discordApiInfo,
                [Frozen] MockHttpMessageHandler handler,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Post, discordApiInfo.OAuth2TokenEndpoint.ToString())
                .WithFormData("client_secret", discordApiInfo.ClientSecret)
                .Respond("application/json", @$"{{""access_token"":""{accessToken}""}}");

                await service.ExchangeOAuth2CodeForToken(code, cancellationToken);

                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldMakeARequestToDiscordsTokenEndpointWithTheGrantType(
                string code,
                string accessToken,
                [Frozen] IDiscordApiInfoService discordApiInfo,
                [Frozen] MockHttpMessageHandler handler,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Post, discordApiInfo.OAuth2TokenEndpoint.ToString())
                .WithFormData("grant_type", "authorization_code")
                .Respond("application/json", @$"{{""access_token"":""{accessToken}""}}");

                await service.ExchangeOAuth2CodeForToken(code, cancellationToken);

                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldMakeARequestToDiscordsTokenEndpointWithTheRedirectUri(
                string code,
                string accessToken,
                [Frozen] IDiscordApiInfoService discordApiInfo,
                [Frozen] MockHttpMessageHandler handler,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Post, discordApiInfo.OAuth2TokenEndpoint.ToString())
                .WithFormData("redirect_uri", discordApiInfo.OAuth2RedirectUri.ToString())
                .Respond("application/json", @$"{{""access_token"":""{accessToken}""}}");

                await service.ExchangeOAuth2CodeForToken(code, cancellationToken);

                handler.VerifyNoOutstandingExpectation();
            }

            [Test, Auto]
            public async Task ShouldReturnTheAccessTokenFromTheBody(
                string code,
                string accessToken,
                [Frozen] IDiscordApiInfoService discordApiInfo,
                [Frozen] MockHttpMessageHandler handler,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Post, discordApiInfo.OAuth2TokenEndpoint.ToString())
                .Respond("application/json", @$"{{""access_token"":""{accessToken}""}}");

                var result = await service.ExchangeOAuth2CodeForToken(code, cancellationToken);

                result.Should().Be(accessToken);
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class GetDiscordUserInfoTests
        {
            [Test, Auto]
            public async Task ShouldSendARequestToTheMeEndpointAndReturnTheUserId(
                Snowflake userId,
                string token,
                [Frozen] IDiscordApiInfoService discordApiInfo,
                [Frozen] MockHttpMessageHandler handler,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                handler
                .Expect(HttpMethod.Get, discordApiInfo.OAuth2UserInfoEndpoint.ToString())
                .WithHeaders("authorization", $"Bearer {token}")
                .Respond("application/json", $@"{{""id"":""{userId.Value}""}}");

                var result = await service.GetDiscordUserInfo(token, cancellationToken);

                result.Id.Should().Be(userId);
                handler.VerifyNoOutstandingExpectation();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class LinkDiscordIdToUser
        {
            [Test, Auto]
            public async Task ShouldCreateALoginsClientWithTheGivenToken(
                Snowflake discordUserId,
                Guid identityUserId,
                string accessToken,
                [Frozen, Substitute] IUsersClientFactory factory,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                await service.LinkDiscordIdToUser(discordUserId, identityUserId, accessToken, cancellationToken);

                factory.Received().Create(Is(accessToken));
            }

            [Test, Auto]
            public async Task ShouldCreateAUserLoginWithTheGivenInfo(
                Snowflake discordUserId,
                Guid identityUserId,
                string accessToken,
                [Frozen, Substitute] IUsersClient usersClient,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                await service.LinkDiscordIdToUser(discordUserId, identityUserId, accessToken, cancellationToken);

                await usersClient.Received().CreateLogin(
                    Is(identityUserId),
                    Is<CreateUserLoginRequest>(userLogin =>
                        userLogin.LoginProvider == "discord" &&
                        userLogin.ProviderKey == discordUserId
                    ),
                    Is(cancellationToken)
                );
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class GetUserIdFromIdentityTokenTests
        {
            [Test, Auto]
            public void ShouldReturnTheUserId(
                [Target] DefaultUserService service
            )
            {
                var token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjRDRDI0ODY0Njc3RTE5REMyMTY5RTVDODRFRjQyQzNDOTVBMzc0OEEiLCJ0eXAiOiJKV1QifQ.eyJuYW1lIjoidGFsZW4uZmlzaGVyQGN5dGhyYWwuY29tIiwic3ViIjoiMjhkOGE1NzUtZmY1Zi00NTZkLThhYmUtODM3ZmI2YTQ1Y2IyIiwicm9sZSI6WyJCYXNpYyIsIkFkbWluaXN0cmF0b3IiXSwibmJmIjoxNjIyMzIwMzA5LCJleHAiOjE2MjIzMjM5MDksImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0LyJ9.";

                var result = service.GetUserIdFromIdentityToken(token);

                result.Should().Be(Guid.Parse("28d8a575-ff5f-456d-8abe-837fb6a45cb2"));
            }
        }

        [TestFixture]
        [Category("Unit")]
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
                var login = new UserLogin { LoginProvider = "discord", Enabled = true };
                var remoteUser = new Identity.Client.User { Id = remoteId, Logins = new List<UserLogin> { login } };
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
                var login = new UserLogin { LoginProvider = "discord", Enabled = true };
                var remoteUser = new Identity.Client.User { Id = remoteId, Logins = new List<UserLogin> { login } };
                var cancellationToken = new CancellationToken(false);

                cache.ContainsKey(Any<Snowflake>()).Returns(false);
                client.GetUserByLoginProviderKey(Any<string>(), Any<string>(), Any<CancellationToken>()).Returns(remoteUser);

                await service.IsUserRegistered(user, cancellationToken);

                cache.Received().Add(Is(user.Id), Is<UserId>(id => id.Id == remoteId && id.Enabled));
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

        [TestFixture]
        [Category("Unit")]
        public class GetIdentityServiceUserIdTests
        {
            [Test, Auto]
            public async Task ShouldFetchTheUserIdFromTheIdentityService(
                Models.User user,
                [Frozen] Identity.Client.User identityUser,
                [Frozen, Substitute] ILoginProvidersClient loginProvidersClient,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                var login = new UserLogin { LoginProvider = "discord", Enabled = true };
                identityUser.Logins.Add(login);
                identityUser.Flags = (long)UserFlags.None;

                var result = await service.GetIdentityServiceUserId(user, cancellationToken);

                result.Id.Should().Be(identityUser.Id);
                result.Debug.Should().BeFalse();
                result.Enabled.Should().BeTrue();

                await loginProvidersClient.Received().GetUserByLoginProviderKey(Is("discord"), Is(user.Id.Value.ToString()), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldSetDebugOnTheUserIdIfUserHasDebugFlag(
                Models.User user,
                [Frozen] Identity.Client.User identityUser,
                [Frozen, Substitute] ILoginProvidersClient loginProvidersClient,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                var login = new UserLogin { LoginProvider = "discord", Enabled = true };
                identityUser.Logins.Add(login);
                identityUser.Flags = (long)UserFlags.Debug;

                var result = await service.GetIdentityServiceUserId(user, cancellationToken);

                result.Id.Should().Be(identityUser.Id);
                result.Debug.Should().BeTrue();
            }

            [Test, Auto]
            public async Task ShouldRetrieveUserIdFromCacheIfPresent(
                Models.User user,
                UserId expectedId,
                [Frozen, Substitute] IUserIdCache userIdCache,
                [Frozen, Substitute] ILoginProvidersClient loginProvidersClient,
                [Target] DefaultUserService service,
                CancellationToken cancellationToken
            )
            {
                userIdCache.TryGetValue(Any<Snowflake>(), out Any<UserId>()).Returns(x =>
                {
                    x[1] = expectedId;
                    return true;
                });

                var result = await service.GetIdentityServiceUserId(user, cancellationToken);

                result.Should().Be(expectedId);
                await loginProvidersClient.DidNotReceive().GetUserByLoginProviderKey(Is("discord"), Is(user.Id.Value.ToString()), Is(cancellationToken));
            }
        }
    }
}
