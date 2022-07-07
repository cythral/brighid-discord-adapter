using System.Text.Json.Serialization;

namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// Metadata returned from the ECS Task Metadata Endpoint V4.
    /// </summary>
    public class TaskMetadata
    {
        /// <summary>
        /// Gets or sets the ARN of the task.
        /// </summary>
        [JsonPropertyName("TaskARN")]
        public string TaskArn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the cluster.
        /// </summary>
        [JsonPropertyName("Cluster")]
        public string Cluster { get; set; } = string.Empty;
    }
}
