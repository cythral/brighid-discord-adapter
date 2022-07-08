using System;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// Represents information about a container within a task.
    /// </summary>
    public class Container
    {
        /// <summary>
        /// Gets or sets the list of network interfaces attached to the task.
        /// </summary>
        [JsonPropertyName("Networks")]
        public Network[] Networks { get; set; } = Array.Empty<Network>();
    }
}
