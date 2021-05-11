using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Router that routes events to controllers.
    /// </summary>
    public interface IEventRouter
    {
        /// <summary>
        /// Routes a gateway event to its controller.
        /// </summary>
        /// <param name="event">The event to route.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Route(IGatewayEvent @event, CancellationToken cancellationToken);
    }
}
