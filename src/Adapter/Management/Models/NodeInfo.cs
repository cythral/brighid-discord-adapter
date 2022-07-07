using System.Net;
using System.Text.Json.Serialization;

namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// Represents information about the adapter node.
    /// </summary>
    public class NodeInfo
    {
        /// <summary>
        /// Gets or sets the IP Address of the node.
        /// </summary>
        [JsonPropertyName("ip_address")]
        [JsonConverter(typeof(IpAddressConverter))]
        public IPAddress IpAddress { get; set; } = IPAddress.None;

        /// <summary>
        /// Gets or sets the shard number.
        /// </summary>
        [JsonPropertyName("shard")]
        public int Shard { get; set; }

        /// <summary>
        /// Gets or sets the id of the deployment this node belongs to.
        /// </summary>
        [JsonPropertyName("deployment_id")]
        public string DeploymentId { get; set; } = string.Empty;
    }
}
