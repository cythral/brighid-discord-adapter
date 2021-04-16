using System.Text.Json.Serialization;

namespace Brighid.Discord.Models
{
    /// <summary>
    /// Connection properties to pass onto the gateway.
    /// </summary>
    public struct ConnectionProperties
    {
        /// <summary>
        /// Gets or sets the operating system in use.
        /// </summary>
        [JsonPropertyName("$os")]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the library name.
        /// </summary>
        [JsonPropertyName("$browser")]
        public string Browser { get; set; }

        /// <summary>
        /// Gets or sets the library name.
        /// </summary>
        [JsonPropertyName("$device")]
        public string Device { get; set; }
    }
}
