using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Users
{
    public class OAuth2ControllerTests
    {
        [Test, Auto]
        public async Task ShouldReturnBadRequestIfAccessTokenCookieNotPresent(
            string code,
            [Substitute] HttpContext httpContext,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.Callback(code);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Test, Auto]
        public async Task ShouldReturnBadRequestIfIdTokenCookieNotPresent(
            string code,
            string accessToken,
            [Substitute] HttpContext httpContext,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            httpContext.Request.Cookies.TryGetValue(Is(".Brighid.AccessToken"), out Any<string?>()).Returns(x =>
            {
                x[1] = accessToken;
                return true;
            });

            httpContext.Request.Cookies.TryGetValue(Is(".Brighid.IdentityToken"), out Any<string?>()).Returns(x =>
            {
                x[1] = null;
                return false;
            });

            var result = await controller.Callback(code);

            result.Should().BeOfType<BadRequestResult>();
        }

        [Test, Auto]
        public async Task ShouldGetTheIdentityUserIdFromTheIdToken(
            string code,
            string accessToken,
            string idToken,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            SetupHttpContext(httpContext, accessToken, idToken);

            await controller.Callback(code);

            userService.Received().GetUserIdFromIdentityToken(Is(idToken));
        }

        [Test, Auto]
        public async Task ShouldExchangeTheCodeForAToken(
            string code,
            string accessToken,
            string idToken,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            SetupHttpContext(httpContext, accessToken, idToken);

            await controller.Callback(code);

            await userService.Received().ExchangeOAuth2CodeForToken(Is(code), Is(httpContext.RequestAborted));
        }

        [Test, Auto]
        public async Task ShouldUseTheTokenToFetchTheUsersDiscordId(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            SetupHttpContext(httpContext, accessToken, idToken);

            await controller.Callback(code);

            await userService.Received().GetDiscordUserId(Is(discordToken), Is(httpContext.RequestAborted));
        }

        [Test, Auto]
        public async Task ShouldLinkTheDiscordIdToTheUser(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Snowflake discordUserId,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserId(Is(discordToken), Any<CancellationToken>()).Returns(discordUserId);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            SetupHttpContext(httpContext, accessToken, idToken);

            await controller.Callback(code);

            await userService.Received().LinkDiscordIdToUser(Is(discordUserId), Is(identityUserId), Is(accessToken), Is(httpContext.RequestAborted));
        }

        private void SetupHttpContext(HttpContext httpContext, string accessToken, string idToken)
        {
            httpContext.Request.Cookies.TryGetValue(Is(".Brighid.AccessToken"), out Any<string?>()).Returns(x =>
            {
                x[1] = accessToken;
                return true;
            });

            httpContext.Request.Cookies.TryGetValue(Is(".Brighid.IdentityToken"), out Any<string?>()).Returns(x =>
            {
                x[1] = idToken;
                return true;
            });
        }
    }
}
