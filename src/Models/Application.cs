using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// OAuth2 Application Info.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// Gets or sets the id of the app.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
        public ulong Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the app.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the icon hash of the app.
        /// </summary>
        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets the description of the app.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an array of rpc origin urls, if rpc is enabled.
        /// </summary>
        [JsonPropertyName("rpc_origins")]
        public string[]? RpcOrigins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a bot can be freely added to any guild.
        /// </summary>
        [JsonPropertyName("bot_public")]
        public bool IsBotPublic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the app's bot will only join upon completion of the full oauth2 code grant flow.
        /// </summary>
        [JsonPropertyName("bot_require_code_grant")]
        public bool BotRequireCodeGrant { get; set; }

        /// <summary>
        /// Gets or sets the url of the app's terms of service.
        /// </summary>
        [JsonPropertyName("terms_of_service_url")]
        public string? TermsOfServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the url of the app's privacy policy.
        /// </summary>
        [JsonPropertyName("privacy_policy_url")]
        public string? PrivacyPolicyUrl { get; set; }

        /// <summary>
        /// Gets or sets the partial user object containing info on the owner of the application.
        /// </summary>
        [JsonPropertyName("owner")]
        public User? Owner { get; set; }

        /// <summary>
        /// Gets or sets the summary field for the store page of its primary sku. (Only valid for games sold on discord).
        /// </summary>
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hex encoded key for verification in interactions and the GameSDK's GetTicket.
        /// </summary>
        [JsonPropertyName("verify_key")]
        public string VerifyKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the team that owns this application.
        /// </summary>
        [JsonPropertyName("team")]
        public Team? Team { get; set; }

        /// <summary>
        /// Gets or sets the id of the guild to which it has been linked. (Only valid for games sold on discord.)
        /// </summary>
        [JsonPropertyName("guild_id")]
        public ulong? GuildId { get; set; }

        /// <summary>
        /// Gets or sets the the id of the "Game SKU" that is created, if exists. (Only valid for games sold on discord.)
        /// </summary>
        [JsonPropertyName("primary_sku_id")]
        public ulong? PrimarySkuId { get; set; }

        /// <summary>
        /// Gets or sets the URL slug that links to the store page. (Only valid for games sold on discord.)
        /// </summary>
        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        /// <summary>
        /// Gets or sets the hash of the image on store embeds. (Only valid for games sold on discord.)
        /// </summary>
        [JsonPropertyName("cover_image")]
        public string? CoverImage { get; set; }

        /// <summary>
        /// Gets or sets the applications' public flags.
        /// </summary>
        [JsonPropertyName("flags")]
        public int Flags { get; set; }
    }
}
