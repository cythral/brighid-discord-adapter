using System;

namespace Brighid.Discord.Adapter.Auth
{
    /// <summary>
    /// Options used for authentication and authorization.
    /// </summary>
    public class AuthOptions
    {
        /// <summary>
        /// Gets or sets the JWT Metadata Address to use for validating tokens.
        /// </summary>
        public Uri MetadataAddress { get; set; } = new Uri("https://identity.brigh.id/.well-known/openid-configuration");

        /// <summary>
        /// Gets or sets issuer to validate tokens against.
        /// </summary>
        public string ValidIssuer { get; set; } = "https://identity.brigh.id/";

        /// <summary>
        /// Gets or sets the clock skew (expiry threshold) in minutes.
        /// </summary>
        public uint ClockSkew { get; set; } = 5;
    }
}
