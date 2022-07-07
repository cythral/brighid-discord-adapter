using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Amazon.ECS;
using Amazon.ECS.Model;

using Microsoft.Extensions.Options;

using HttpClient = System.Net.Http.HttpClient;
using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;
using Task = System.Threading.Tasks.Task;

namespace Brighid.Discord.Adapter.Management
{
    /// <inheritdoc />
    public class NodeService : INodeService
    {
        private readonly HttpClient httpClient;
        private readonly IAmazonECS ecs;
        private readonly IDnsService dns;
        private readonly AdapterOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeService" /> class.
        /// </summary>
        /// <param name="httpClient">HTTP Client to fetch instance metadata with.</param>
        /// <param name="ecs">ECS Service to fetch task info from.</param>
        /// <param name="dns">DNS Service to fetch IP addresses with.</param>
        /// <param name="options">Adapter options.</param>
        public NodeService(
            HttpClient httpClient,
            IAmazonECS ecs,
            IDnsService dns,
            IOptions<AdapterOptions> options
        )
        {
            this.httpClient = httpClient;
            this.ecs = ecs;
            this.dns = dns;
            this.options = options.Value;
        }

        /// <inheritdoc />
        public IPAddress GetIpAddress()
        {
            var query = from @interface in NetworkInterface.GetAllNetworkInterfaces()
                        where @interface.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                        from ip in @interface.GetIPProperties().UnicastAddresses
                        where ip.Address.AddressFamily == AddressFamily.InterNetwork
                        select ip.Address;

            return query.First();
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

        /// <inheritdoc />
        public async Task<IEnumerable<NodeInfo>> GetPeers(CancellationToken cancellationToken)
        {
            var addresses = await dns.GetIPAddresses(options.Host, cancellationToken);
            var nodes = from address in addresses
                        select httpClient.GetFromJsonAsync<NodeInfo>($"http://{address}/node", cancellationToken: cancellationToken);

            return await Task.WhenAll(nodes);
        }
    }
}
