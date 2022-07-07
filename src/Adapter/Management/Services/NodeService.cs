using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.ECS;
using Amazon.ECS.Model;

using Microsoft.Extensions.Options;

using HttpClient = System.Net.Http.HttpClient;

namespace Brighid.Discord.Adapter.Management
{
    /// <inheritdoc />
    public class NodeService : INodeService
    {
        private readonly HttpClient httpClient;
        private readonly IAmazonECS ecs;
        private readonly AdapterOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeService" /> class.
        /// </summary>
        /// <param name="httpClient">HTTP Client to fetch instance metadata with.</param>
        /// <param name="ecs">ECS Service to fetch task info from.</param>
        /// <param name="options">Adapter options.</param>
        public NodeService(
            HttpClient httpClient,
            IAmazonECS ecs,
            IOptions<AdapterOptions> options
        )
        {
            this.httpClient = httpClient;
            this.ecs = ecs;
            this.options = options.Value;
        }

        /// <inheritdoc />
        public async Task<string> GetDeploymentId(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (options.TaskMetadataUrl == null)
            {
                return "local";
            }

            var taskMetadata = await httpClient.GetFromJsonAsync<TaskMetadata>(options.TaskMetadataUrl, cancellationToken: cancellationToken);
            var describeTasksRequest = new DescribeTasksRequest { Cluster = taskMetadata!.Cluster, Tasks = new() { taskMetadata!.TaskArn } };
            var describeTasksResponse = await ecs.DescribeTasksAsync(describeTasksRequest, cancellationToken);
            return describeTasksResponse.Tasks.First().StartedBy;
        }
    }
}
