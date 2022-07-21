using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Gateway;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Management
{
    /// <inheritdoc />
    public class TrafficShifter : ITrafficShifter
    {
        private readonly IAdapterContext context;
        private readonly HttpClient httpClient;
        private readonly ILogger<TrafficShifter> logger;
        private readonly StringContent content = new(GatewayState.Stopped.ToString());

        /// <summary>
        /// Initializes a new instance of the <see cref="TrafficShifter"/> class.
        /// </summary>
        /// <param name="context">Adapter context.</param>
        /// <param name="httpClient">Client used for making HTTP calls for traffic shifting.</param>
        /// <param name="logger">Service used for logging messages.</param>
        public TrafficShifter(
            IAdapterContext context,
            HttpClient httpClient,
            ILogger<TrafficShifter> logger
        )
        {
            this.context = context;
            this.httpClient = httpClient;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task PerformTrafficShift(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentNode = context.Get<NodeInfo>();
            var peers = context.Get<IEnumerable<NodeInfo>>();

            var tasks = from peer in peers
                        where peer.Shard == currentNode.Shard
                        select Stop(peer.IpAddress, cancellationToken);

            await Task.WhenAll(tasks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task Stop(IPAddress address, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri($"http://{address}/node/gateway/state"),
                    Version = new Version(2, 0),
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact,
                    Content = content,
                };

                var response = await httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Successfully shifted traffic from {@address}", address);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Was unable to shift traffic from {@address}", address);
            }
        }
    }
}
