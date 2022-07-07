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
        /// Gets the deployment ID.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>Info about the node.</returns>
        Task<string> GetDeploymentId(CancellationToken cancellationToken);
    }
}
