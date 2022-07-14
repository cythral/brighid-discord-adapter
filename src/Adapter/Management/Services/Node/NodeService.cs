using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.ECS;
using Amazon.ECS.Model;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using HttpClient = System.Net.Http.HttpClient;
using Task = System.Threading.Tasks.Task;

namespace Brighid.Discord.Adapter.Management
{
    /// <inheritdoc />
    public class NodeService : INodeService
    {
        private readonly HttpClient httpClient;
        private readonly IAmazonECS ecs;
        private readonly IDnsService dns;
        private readonly ILogger<NodeService> logger;
        private readonly AdapterOptions options;
        private TaskMetadata? metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeService" /> class.
        /// </summary>
        /// <param name="httpClient">HTTP Client to fetch instance metadata with.</param>
        /// <param name="ecs">ECS Service to fetch task info from.</param>
        /// <param name="dns">DNS Service to fetch IP addresses with.</param>
        /// <param name="options">Adapter options.</param>
        /// <param name="logger">Service used for logging messages.</param>
        public NodeService(
            HttpClient httpClient,
            IAmazonECS ecs,
            IDnsService dns,
            IOptions<AdapterOptions> options,
            ILogger<NodeService> logger
        )
        {
            this.httpClient = httpClient;
            this.ecs = ecs;
            this.dns = dns;
            this.logger = logger;
            this.options = options.Value;
        }

        /// <inheritdoc />
        public async Task<IPAddress> GetIpAddress(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var taskMetadata = await GetTaskMetadata(cancellationToken);

            return taskMetadata == null
                    ? IPAddress.None
                    : IPAddress.Parse(taskMetadata.Containers[0].Networks[0].IPv6Addresses[0]);
        }

        /// <inheritdoc />
        public async Task<string> GetDeploymentId(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var taskMetadata = await GetTaskMetadata(cancellationToken);
            if (taskMetadata == null)
            {
                return "local";
            }

            var describeTasksRequest = new DescribeTasksRequest { Cluster = taskMetadata!.Cluster, Tasks = new() { taskMetadata!.TaskArn } };
            var describeTasksResponse = await ecs.DescribeTasksAsync(describeTasksRequest, cancellationToken);
            return describeTasksResponse.Tasks.First().StartedBy;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<NodeInfo>> GetPeers(IPAddress currentIp, CancellationToken cancellationToken)
        {
            var addresses = await dns.GetIPAddresses(options.Host, cancellationToken);
            logger.LogInformation("Peer IP Addresses: {@peers}", string.Join(", ", addresses.Select(address => address.ToString())));
            var nodes = from address in addresses
                        where !address.Equals(currentIp)
                        select httpClient.GetFromJsonAsync<NodeInfo>($"http://[{address}]/node", cancellationToken: cancellationToken);

            try
            {
                var result = await Task.WhenAll(nodes);
                logger.LogInformation("Received list of peers: {@peers}", result);
                return result;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Got exception when retrieving peer list.");
                return Array.Empty<NodeInfo>();
            }
        }

        private async Task<TaskMetadata?> GetTaskMetadata(CancellationToken cancellationToken)
        {
            if (options.TaskMetadataUrl == null)
            {
                return null;
            }

            metadata ??= await httpClient.GetFromJsonAsync<TaskMetadata>(options.TaskMetadataUrl, cancellationToken: cancellationToken);
            logger.LogInformation("Retrieved task metadata {@metadata}", metadata);
            return metadata;
        }
    }
}
