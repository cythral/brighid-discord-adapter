using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Gateway
{
    /// <summary>
    /// Represents a gateway event.
    /// </summary>
    public interface IGatewayEvent
    {
        /// <summary>
        /// Handles the gateway event.
        /// </summary>
        /// <param name="gateway">The gateway the event came from.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Handle(IGatewayService gateway, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
