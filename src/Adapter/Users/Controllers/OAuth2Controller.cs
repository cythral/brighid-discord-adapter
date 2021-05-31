using System;
using System.Net;
using System.Threading.Tasks;

using Brighid.Identity.Client;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Users
{
    /// <summary>
    /// Controller for various OAuth2 Activities.
    /// </summary>
    [Route("/oauth2")]
    public class OAuth2Controller : Controller
    {
        private readonly IUserService userService;
        private readonly ILogger<OAuth2Controller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2Controller" /> class.
        /// </summary>
        /// <param name="userService">Service for user operations.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public OAuth2Controller(
            IUserService userService,
            ILogger<OAuth2Controller> logger
        )
        {
            this.userService = userService;
            this.logger = logger;
        }

        /// <summary>
        /// The callback route is called after an OAuth2 exchange is made with the intent to
        /// link a discord account with a Brighid Identity account.  We need to retrieve the user's Discord ID
        /// then create a User Login with it on Brighid Identity.
        /// </summary>
        /// <param name="code">The OAuth2 Code to exchange for a token from Discord.</param>
        /// <returns>The HTTP Response, Ok is successful or BadRequest if not.</returns>
        [Route("callback")]
        public async Task<IActionResult> Callback([FromQuery(Name = "code")] string code)
        {
            var cookies = HttpContext.Request.Cookies;
            if (!cookies.TryGetValue(".Brighid.AccessToken", out var accessToken) || accessToken == null)
            {
                return BadRequest();
            }

            if (!cookies.TryGetValue(".Brighid.IdentityToken", out var idToken) || idToken == null)
            {
                return BadRequest();
            }

            var discordUser = (Models.User?)null;
            var status = AccountLinkStatus.Success;

            try
            {
                var identityUserId = userService.GetUserIdFromIdentityToken(idToken);
                var token = await userService.ExchangeOAuth2CodeForToken(code, HttpContext.RequestAborted);
                discordUser = await userService.GetDiscordUserInfo(token, HttpContext.RequestAborted);
                await userService.LinkDiscordIdToUser(discordUser!.Value.Id, identityUserId, accessToken, HttpContext.RequestAborted);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;
            }
            catch (ApiException exception) when (exception.StatusCode == (int)HttpStatusCode.Conflict)
            {
                logger.LogError("Received conflict exception while attempting to link an account: {@exception}", exception);
                status = AccountLinkStatus.AlreadyLinked;
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
            }
            catch (Exception exception)
            {
                logger.LogError("Received an error while attempting to link an account: {@exception}", exception);
                status = AccountLinkStatus.Failed;
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return View("~/Users/Views/AccountLink.cshtml", new AccountLinkViewModel
            {
                Status = status,
                DiscordUsername = discordUser?.Name,
                DiscordDiscriminator = discordUser?.Discriminator,
                DiscordUserId = discordUser?.Id,
                DiscordAvatarHash = discordUser?.Avatar,
            });
        }
    }
}
