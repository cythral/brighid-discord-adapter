using System;

namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// Service for getting info about the discord API.
    /// </summary>
    public interface IDiscordApiInfoService
    {
        /// <summary>
        /// Gets the version of the API to use.
        /// </summary>
        string ApiVersion { get; }

        /// <summary>
        /// Gets the client id used to authenticate with the discord API.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// Gets the client secret used to authenticate with the discord API.
        /// </summary>
        string ClientSecret { get; }

        /// <summary>
        /// Gets the base API URL to use for interacting with the discord API.
        /// </summary>
        Uri ApiBaseUrl { get; }

        /// <summary>
        /// Gets the OAuth2 Token endpoint for authenticating with the discord API.
        /// </summary>
        Uri OAuth2TokenEndpoint { get; }

        /// <summary>
        /// Gets the OAuth2 User Info endpoint for authenticating with the discord API.
        /// </summary>
        Uri OAuth2UserInfoEndpoint { get; }

        /// <summary>
        /// Gets the OAuth2 Redirect URI endpoint for authenticating with the discord API.
        /// </summary>
        Uri OAuth2RedirectUri { get; }
    }
}
