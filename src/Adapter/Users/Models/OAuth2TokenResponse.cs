using System.Text.Json.Serialization;

namespace Brighid.Discord.Adapter.Users
{
    /// <summary>
    /// Response returned from an OAuth2 Token Endpoint on Discord.
    /// </summary>
    public struct OAuth2TokenResponse
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the type of token returned.
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the time in seconds from now the token expires at.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public uint ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the scope of the access token.
        /// </summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}
