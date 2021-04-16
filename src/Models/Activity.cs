using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// A user activity.
    /// </summary>
    public struct Activity
    {
        /// <summary>
        /// Gets or sets the activity's name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets activity type.
        /// </summary>
        [JsonPropertyName("type")]
        public ActivityType Type { get; set; }

        /// <summary>
        /// Gets or sets stream url, is validated when type is 1.
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
