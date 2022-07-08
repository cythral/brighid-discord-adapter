using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Management
{
    /// <summary>
    /// Service used for node-specific operations.
    /// </summary>
    public interface INodeService
    {
        /// <summary>
        /// Gets the IP address of the node.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The node's IP address.</returns>
        Task<IPAddress> GetIpAddress(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the deployment ID.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Info about the node.</returns>
        Task<string> GetDeploymentId(CancellationToken cancellationToken);

        /// <summary>
        /// Get a list of peer nodes.
        /// </summary>
        /// <param name="currentIp">IP address of the current node.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A list of peer adapter nodes.</returns>
        Task<IEnumerable<NodeInfo>> GetPeers(IPAddress currentIp, CancellationToken cancellationToken);
    }
}
