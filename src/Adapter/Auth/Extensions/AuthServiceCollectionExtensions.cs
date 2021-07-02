using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Brighid.Discord.Adapter.Auth;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring auth services on service collections.
    /// </summary>
    public static class AuthServiceCollectionExtensions
    {
        /// <summary>
        /// Adds auth services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configure">Configuration delegate to configure auth with.</param>
        public static void ConfigureAuthServices(this IServiceCollection services, Action<AuthOptions> configure)
        {
            var authOptions = new AuthOptions();
            configure(authOptions);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[JwtRegisteredClaimNames.Sub] = ClaimTypes.Name;

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RefreshOnIssuerKeyNotFound = true;
                options.RequireHttpsMetadata = authOptions.MetadataAddress.Scheme == "https";
                options.BackchannelHttpHandler = new Http2AuthMessageHandler();
                options.MetadataAddress = authOptions.MetadataAddress.ToString();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    RoleClaimType = "role",
                    ValidIssuer = authOptions.ValidIssuer,
                    ClockSkew = TimeSpan.FromMinutes(authOptions.ClockSkew),
                };
            });
        }
    }
}
