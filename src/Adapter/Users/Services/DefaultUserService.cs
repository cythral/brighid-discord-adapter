using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Models;
using Brighid.Identity.Client;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Adapter.Users
{
    /// <inheritdoc />
    public class DefaultUserService : IUserService
    {
        private readonly IUserIdCache userIdCache;
        private readonly ILoginProvidersClient loginProvidersClient;
        private readonly IUsersClientFactory usersClientFactory;
        private readonly JwtSecurityTokenHandler tokenHandler;
        private readonly HttpClient httpClient;
        private readonly AdapterOptions adapterOptions;
        private readonly ILogger<DefaultUserService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUserService" /> class.
        /// </summary>
        /// <param name="userIdCache">Cache for user ID lookups.</param>
        /// <param name="loginProvidersClient">Client used to manage login providers and linked users in Brighid Identity.</param>
        /// <param name="httpClient">Client used to make arbitrary HTTP requests.</param>
        /// <param name="usersClientFactory">Factory for creating Brighid Identity Users clients with.</param>
        /// <param name="tokenHandler">The security token handler used for reading ID Token JWTs.</param>
        /// <param name="adapterOptions">Misc options used across adapter services.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultUserService(
            IUserIdCache userIdCache,
            ILoginProvidersClient loginProvidersClient,
            HttpClient httpClient,
            IUsersClientFactory usersClientFactory,
            JwtSecurityTokenHandler tokenHandler,
            IOptions<AdapterOptions> adapterOptions,
            ILogger<DefaultUserService> logger
        )
        {
            this.userIdCache = userIdCache;
            this.loginProvidersClient = loginProvidersClient;
            this.usersClientFactory = usersClientFactory;
            this.tokenHandler = tokenHandler;
            this.httpClient = httpClient;
            this.adapterOptions = adapterOptions.Value;
            this.logger = logger;
        }

#pragma warning disable IDE0004 // cast on line 52 is needed
        /// <inheritdoc />
        public async Task<string> ExchangeOAuth2CodeForToken(string code, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new HttpRequestMessage
            {
                RequestUri = adapterOptions.OAuth2TokenEndpoint,
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string?, string?>>)new Dictionary<string, string?>
                {
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["client_id"] = adapterOptions.ClientId,
                    ["client_secret"] = adapterOptions.ClientSecret,
                    ["redirect_uri"] = adapterOptions.OAuth2RedirectUri.ToString(),
                }),
            };

            var response = await httpClient.SendAsync(request, cancellationToken);
            var tokenResponse = await response.Content.ReadFromJsonAsync<OAuth2TokenResponse>(cancellationToken: cancellationToken);
            return tokenResponse.AccessToken;
        }
#pragma warning disable IDE0004

        /// <inheritdoc />
        public async Task<Models.User> GetDiscordUserInfo(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = adapterOptions.OAuth2UserInfoEndpoint,
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.SendAsync(request, cancellationToken);
            var user = await response.Content.ReadFromJsonAsync<Models.User>(cancellationToken: cancellationToken);
            return user;
        }

        /// <inheritdoc />
        public async Task LinkDiscordIdToUser(Snowflake discordUserId, Guid identityUserId, string accessToken, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var client = usersClientFactory.Create(accessToken);
            var userLogin = new CreateUserLoginRequest { LoginProvider = "discord", ProviderKey = discordUserId };
            await client.CreateLogin(identityUserId, userLogin, cancellationToken);
        }

        /// <inheritdoc />
        public Guid GetUserIdFromIdentityToken(string identityToken)
        {
            var jwt = tokenHandler.ReadJwtToken(identityToken);
            return Guid.Parse(jwt.Subject);
        }

        /// <inheritdoc />
        public async Task<bool> IsUserRegistered(Models.User user, CancellationToken cancellationToken)
        {
            if (userIdCache.ContainsKey(user.Id))
            {
                logger.LogInformation("Given userId:{@id} found in cache", user.Id);
                return true;
            }

            try
            {
                var result = await loginProvidersClient.GetUserByLoginProviderKey("discord", user.Id, cancellationToken);
                userIdCache.Add(user.Id, result.Id);
                return true;
            }
            catch (ApiException exception)
            {
                logger.LogError("Received exception from Brighid Identity: {@exception}", exception);
                return false;
            }
        }

        /// <inheritdoc />
        public async ValueTask<Guid> GetIdentityServiceUserId(Models.User user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (userIdCache.TryGetValue(user.Id, out var cachedId))
            {
                return cachedId;
            }

            var result = await loginProvidersClient.GetUserByLoginProviderKey("discord", user.Id, cancellationToken);
            userIdCache.Add(user.Id, result.Id);
            return result.Id;
        }
    }
}
