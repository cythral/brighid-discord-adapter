using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Sent by the client to indicate a presence or status update.
    /// </summary>
    public struct PresenceUpdate
    {
        /// <summary>
        /// Gets or sets unix time (in milliseconds) of when the client went idle, or null if the client is not idle.
        /// </summary>
        [JsonPropertyName("since")]
        public int? Since { get; set; }

        /// <summary>
        /// Gets or sets the user's new status.
        /// </summary>
        [JsonPropertyName("status")]
        public Status Status { get; set; }

        /// <summary>
        /// Gets or sets the user's current activities.
        /// </summary>
        [JsonPropertyName("activities")]
        public Activity[] Activities { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the client is afk.
        /// </summary>
        [JsonPropertyName("fk")]
        public bool AwayFromKeyboard { get; set; }
    }
}
