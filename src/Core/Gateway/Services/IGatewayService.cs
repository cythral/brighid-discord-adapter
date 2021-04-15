using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Gateway
{
    /// <summary>
    /// Service to interface with the discord gateway.
    /// </summary>
    public interface IGatewayService
    {
        /// <summary>
        /// Starts the gateway service and begin listening for messages.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Run(CancellationToken cancellationToken = default);
    }
}
