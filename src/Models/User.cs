using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Discord user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the user's id.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
        public ulong Id { get; set; }

        /// <summary>
        /// Gets or sets the user's username, not unique across the platform.
        /// </summary>
        [JsonPropertyName("username")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's 4-digit discord-tag.
        /// </summary>
        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's avatar hash.
        /// </summary>
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the user belongs to an OAuth2 application.
        /// </summary>
        [JsonPropertyName("bot")]
        public bool? IsBot { get; set; }

        /// <summary>
        /// Gets or sets whether the user is an Official Discord System user (part of the urgent message system).
        /// </summary>
        [JsonPropertyName("system")]
        public bool? IsSystem { get; set; }

        /// <summary>
        /// Gets or sets whether the user has two factor enabled on their account.
        /// </summary>
        [JsonPropertyName("mfa_enabled")]
        public bool? IsMfaEnabled { get; set; }

        /// <summary>
        /// Gets or sets the user's chosen language option.
        /// </summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        /// <summary>
        /// Gets or sets whether the email on this account has been verified.
        /// </summary>
        [JsonPropertyName("verified")]
        public bool? IsVerified { get; set; }

        /// <summary>
        /// Gets or sets the user's email.
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the flags on a user's account.
        /// </summary>
        [JsonPropertyName("flags")]
        public int? Flags { get; set; }

        /// <summary>
        /// Gets or sets the type of Nitro subscription on a user's account.
        /// </summary>
        [JsonPropertyName("premium_type")]
        public int? PremiumType { get; set; }

        /// <summary>
        /// Gets or sets the public flags on a user's account.
        /// </summary>
        [JsonPropertyName("public_flags")]
        public int? PublicFlags { get; set; }
    }
}
