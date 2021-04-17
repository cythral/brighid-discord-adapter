using System;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A member of a guild.
    /// </summary>
    public struct GuildMember
    {
        /// <summary>
        /// Gets or sets the user this guild member represents.
        /// </summary>
        [JsonPropertyName("user")]
        public User? User { get; set; }

        /// <summary>
        /// Gets or sets this users guild nickname.
        /// </summary>
        [JsonPropertyName("nick")]
        public string? NickName { get; set; }

        /// <summary>
        /// Gets or sets an array of role object ids.
        /// </summary>
        [JsonPropertyName("roles")]
        public Snowflake[] Roles { get; set; }

        /// <summary>
        /// Gets or sets when the user joined the guild.
        /// </summary>
        [JsonPropertyName("joined_at")]
        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// Gets or sets when the user started boosting the guild.
        /// </summary>
        [JsonPropertyName("premium_since")]
        public DateTime? PremiumSince { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is deafened in voice channels.
        /// </summary>
        [JsonPropertyName("deaf")]
        public bool Deaf { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is muted in voice channels.
        /// </summary>
        [JsonPropertyName("mute")]
        public bool Mute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has not yet passed the guild's Membership Screening requirements.
        /// </summary>
        [JsonPropertyName("pending")]
        public bool? Pending { get; set; }

        /// <summary>
        /// Gets or sets total permissions of the member in the channel, including overrides, returned when in the interaction object.
        /// </summary>
        [JsonPropertyName("permissions")]
        public string? Permissions { get; set; }
    }
}
