using System;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// Information about a task's network returned from the metadata endpoint v4.
    /// </summary>
    public class Network
    {
        /// <summary>
        /// Gets or sets the list of ip addresses.
        /// </summary>
        [JsonPropertyName("IPv6Addresses")]
        public string[] IPv6Addresses { get; set; } = Array.Empty<string>();
    }
}
