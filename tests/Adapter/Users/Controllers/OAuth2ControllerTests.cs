using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Identity.Client;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

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
            SetupTempData(controller);

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
            SetupTempData(controller);

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
            SetupTempData(controller);

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
            SetupTempData(controller);

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
            SetupTempData(controller);

            await controller.Callback(code);

            await userService.Received().GetDiscordUserInfo(Is(discordToken), Is(httpContext.RequestAborted));
        }

        [Test, Auto]
        public async Task ShouldLinkTheDiscordIdToTheUser(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            await controller.Callback(code);

            await userService.Received().LinkDiscordIdToUser(Is(discordUser.Id), Is(identityUserId), Is(accessToken), Is(httpContext.RequestAborted));
        }

        [Test, Auto]
        public async Task ShouldReturnTheAccountLinkSuccessView(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            var result = await controller.Callback(code) as ViewResult;

            result.Should().NotBeNull();
            result!.ViewName.Should().Be("~/Users/Views/AccountLink.cshtml");
        }

        [Test, Auto]
        public async Task ShouldSetDiscordUserIdOnTheModelView(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            var result = await controller.Callback(code) as ViewResult;

            result.Should().NotBeNull();
            var model = (AccountLinkViewModel)(result!.Model!);
            model.DiscordUserId.Should().Be(discordUser.Id);
        }

        [Test, Auto]
        public async Task ShouldSetDiscordAvatarHashOnTheModelView(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            var result = await controller.Callback(code) as ViewResult;

            result.Should().NotBeNull();
            var model = (AccountLinkViewModel)(result!.Model!);
            model.DiscordAvatarHash.Should().Be(discordUser.Avatar);
        }

        [Test, Auto]
        public async Task ShouldSetDiscordUsernameOnTheModelView(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            var result = await controller.Callback(code) as ViewResult;

            result.Should().NotBeNull();
            var model = (AccountLinkViewModel)(result!.Model!);
            model.DiscordUsername.Should().Be(discordUser.Name);
        }

        [Test, Auto]
        public async Task ShouldSetDiscordDiscriminatorOnTheModelView(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            var result = await controller.Callback(code) as ViewResult;

            result.Should().NotBeNull();
            var model = (AccountLinkViewModel)(result!.Model!);
            model.DiscordDiscriminator.Should().Be(discordUser.Discriminator);
        }

        [Test, Auto]
        public async Task ShouldSetStatusToSuccessOnTheModelView(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            var result = await controller.Callback(code) as ViewResult;

            result.Should().NotBeNull();
            var model = (AccountLinkViewModel)(result!.Model!);
            model.Status.Should().Be(AccountLinkStatus.Success);
        }

        [Test, Auto]
        public async Task ShouldHaveCreatedStatusCodeOnSuccess(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            await controller.Callback(code);

            httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        }

        [Test, Auto]
        public async Task ShouldSetStatusToFailedOnTheModelViewIfThereWasAnException(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            userService.LinkDiscordIdToUser(Any<Snowflake>(), Any<Guid>(), Any<string>(), Any<CancellationToken>()).Throws<Exception>();
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            var result = await controller.Callback(code) as ViewResult;

            result.Should().NotBeNull();
            var model = (AccountLinkViewModel)(result!.Model!);
            model.Status.Should().Be(AccountLinkStatus.Failed);
        }

        [Test, Auto]
        public async Task ShouldHaveInternalServerErrorStatusCodeIfExceptionWasThrown(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);
            userService.LinkDiscordIdToUser(Any<Snowflake>(), Any<Guid>(), Any<string>(), Any<CancellationToken>()).Throws<Exception>();
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            await controller.Callback(code);

            httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        [Test, Auto]
        public async Task ShouldSetStatusToAlreadyLinkedOnTheModelViewIfThereWasAnApiExceptionWithStatus409(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);

            var exception = new ApiException(string.Empty, (int)HttpStatusCode.Conflict, string.Empty, null!, null!);
            userService.LinkDiscordIdToUser(Any<Snowflake>(), Any<Guid>(), Any<string>(), Any<CancellationToken>()).Throws(exception);
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            var result = await controller.Callback(code) as ViewResult;

            result.Should().NotBeNull();
            var model = (AccountLinkViewModel)(result!.Model!);
            model.Status.Should().Be(AccountLinkStatus.AlreadyLinked);
        }

        [Test, Auto]
        public async Task ShouldHaveConflictStatusCodeIfAlreadyLinked(
            string code,
            string accessToken,
            string idToken,
            string discordToken,
            Models.User discordUser,
            Guid identityUserId,
            [Substitute] HttpContext httpContext,
            [Frozen, Substitute] IUserService userService,
            [Target] OAuth2Controller controller
        )
        {
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            userService.GetDiscordUserInfo(Is(discordToken), Any<CancellationToken>()).Returns(discordUser);
            userService.GetUserIdFromIdentityToken(Is(idToken)).Returns(identityUserId);
            userService.ExchangeOAuth2CodeForToken(Is(code), Any<CancellationToken>()).Returns(discordToken);

            var exception = new ApiException(string.Empty, (int)HttpStatusCode.Conflict, string.Empty, null!, null!);
            userService.LinkDiscordIdToUser(Any<Snowflake>(), Any<Guid>(), Any<string>(), Any<CancellationToken>()).Throws(exception);
            SetupHttpContext(httpContext, accessToken, idToken);
            SetupTempData(controller);

            await controller.Callback(code);

            httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
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

        private void SetupTempData(Controller controller)
        {
            var tempDataProvider = Substitute.For<ITempDataProvider>();
            var tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
            var tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());
            controller.TempData = tempData;
        }
    }
}
