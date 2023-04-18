using System;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Options;

namespace Brighid.Discord.Adapter.Management
{
    /// <inheritdoc />
    public class DiscordApiInfoService : IDiscordApiInfoService
    {
        private readonly AdapterOptions adapterOptions;
        private Uri? apiBaseUrl;
        private Uri? oauth2TokenEndpoint;
        private Uri? oauth2UserInfoEndpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordApiInfoService" /> class.
        /// </summary>
        /// <param name="adapterOptions">Options to use for the adapter.</param>
        public DiscordApiInfoService(
            IOptions<AdapterOptions> adapterOptions
        )
        {
            this.adapterOptions = adapterOptions.Value;
        }

        /// <inheritdoc />
        public string ClientId => adapterOptions.ClientId;

        /// <inheritdoc />
        public string ClientSecret => adapterOptions.ClientSecret;

        /// <inheritdoc />
        public Uri ApiBaseUrl => apiBaseUrl ??= CombineUris(adapterOptions.DiscordApiBaseUrl, adapterOptions.ApiVersion);

        /// <inheritdoc />
        public Uri OAuth2TokenEndpoint => oauth2TokenEndpoint ??= CombineUris(ApiBaseUrl, "/oauth2/token");

        /// <inheritdoc />
        public Uri OAuth2UserInfoEndpoint => oauth2UserInfoEndpoint ??= CombineUris(ApiBaseUrl, "/users/@me");

        /// <inheritdoc />
        public Uri OAuth2RedirectUri => adapterOptions.OAuth2RedirectUri;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Uri CombineUris(Uri baseUri, string relative)
        {
            var baseUrl = baseUri.ToString().TrimEnd('/');
            return new Uri(baseUrl + $"/{relative.TrimStart('/')}");
        }
    }
}
