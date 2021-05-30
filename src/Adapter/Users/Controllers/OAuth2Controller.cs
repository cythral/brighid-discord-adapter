using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Brighid.Discord.Adapter.Users
{
    /// <summary>
    /// Controller for various OAuth2 Activities.
    /// </summary>
    [Route("/oauth2")]
    public class OAuth2Controller : Controller
    {
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2Controller" /> class.
        /// </summary>
        /// <param name="userService">Service for user operations.</param>
        public OAuth2Controller(
            IUserService userService
        )
        {
            this.userService = userService;
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

            var identityUserId = userService.GetUserIdFromIdentityToken(idToken);
            var token = await userService.ExchangeOAuth2CodeForToken(code, HttpContext.RequestAborted);
            var discordUser = await userService.GetDiscordUserInfo(token, HttpContext.RequestAborted);
            await userService.LinkDiscordIdToUser(discordUser.Id, identityUserId, accessToken, HttpContext.RequestAborted);

            return View("~/Users/Views/AccountLinkSuccess.cshtml", new AccountLinkSuccessViewModel
            {
                DiscordUserId = discordUser.Id,
                DiscordAvatarHash = discordUser.Avatar,
            });
        }
    }
}
