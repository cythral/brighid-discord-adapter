using System;

using Destructurama.Attributed;

namespace Brighid.Discord.Adapter
{
    /// <summary>
    /// Miscellaneous config used across adapter services.
    /// </summary>
    public class AdapterOptions
    {
        /// <summary>
        /// Gets or sets the auth scheme to use when making requests to the REST API.
        /// </summary>
        public string AuthScheme { get; set; } = "Bot";

        /// <summary>
        /// Gets or sets the token to authenticate against the gateway and REST API with.
        /// </summary>
        [NotLogged]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Client ID used for OAuth2 Operations on the Discord API.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Client Secret used for OAuth2 Operations on the Discord API.
        /// </summary>
        [NotLogged]
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Registration URL sent to unregistered users on mention.
        /// </summary>
        public string RegistrationUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the OAuth2 User Info Endpoint.
        /// </summary>
        public Uri OAuth2UserInfoEndpoint { get; set; } = new Uri("https://discord.com/api/users/@me");

        /// <summary>
        /// Gets or sets the Discord OAuth2 Token Endpoint.
        /// </summary>
        public Uri OAuth2TokenEndpoint { get; set; } = new Uri("https://discord.com/api/oauth2/token");

        /// <summary>
        /// Gets or sets the Redirect URI used for Discord OAuth2.
        /// </summary>
        public Uri OAuth2RedirectUri { get; set; } = new Uri("http://localhost/oauth2/callback");
    }
}
