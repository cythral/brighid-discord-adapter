using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Information on the current session start limit.
    /// </summary>
    public struct SessionStartLimit
    {
        /// <summary>
        /// Gets or sets the total number of session starts the current user is allowed.
        /// </summary>
        [JsonPropertyName("total")]
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets remaining number of session starts the current user is allowed.
        /// </summary>
        [JsonPropertyName("remaining")]
        public int Remaining { get; set; }

        /// <summary>
        /// Gets or sets number of milliseconds after which the limit resets.
        /// </summary>
        [JsonPropertyName("reset_after")]
        public int ResetAfter { get; set; }

        /// <summary>
        /// Gets or sets the number of identify requests allowed per 5 seconds.
        /// </summary>
        [JsonPropertyName("max_concurrency")]
        public int MaxConcurrency { get; set; }
    }
}
