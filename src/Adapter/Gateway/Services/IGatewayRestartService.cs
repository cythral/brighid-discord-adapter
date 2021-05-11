using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <summary>
    /// Service to restart the gateway.
    /// </summary>
    public interface IGatewayRestartService
    {
        /// <summary>
        /// Restarts the <paramref name="gateway" />.
        /// </summary>
        /// <param name="gateway">The gateway to restart.</param>
        /// <param name="resume">Whether to resume after a restart or identify.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Restart(IGatewayService gateway, bool resume, CancellationToken cancellationToken);
    }
}
