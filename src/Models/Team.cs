using System;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A Team that owns an application.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// Gets or sets the hash of the image of the team's icon.
        /// </summary>
        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets the unique id of the team.
        /// </summary>
        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        /// <summary>
        /// Gets or sets the members of the team.
        /// </summary>
        [JsonPropertyName("members")]
        public TeamMember[] Members { get; set; } = Array.Empty<TeamMember>();

        /// <summary>
        /// Gets or sets the user id of the current team owner.
        /// </summary>
        [JsonPropertyName("owner_user_id")]
        public ulong OwnerUserId { get; set; }
    }
}
