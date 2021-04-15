using System;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A member of a team.
    /// </summary>
    public class TeamMember
    {
        /// <summary>
        /// Gets or sets the user's membership state on the team.
        /// </summary>
        [JsonPropertyName("membership_state")]
        public int MembershipState { get; set; }

        /// <summary>
        /// Gets or sets the team member's permissions.
        /// </summary>
        [JsonPropertyName("permissions")]
        public string[] Permissions { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the id of the parent team of which they are a member.
        /// </summary>
        [JsonPropertyName("team_id")]
        public ulong TeamId { get; set; }

        /// <summary>
        /// Gets or sets the partial user object including the avatar, discriminator, id, and username.
        /// </summary>
        [JsonPropertyName("user")]
        public User? User { get; set; }
    }
}
