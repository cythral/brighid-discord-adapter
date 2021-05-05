using System.Threading;
using System.Threading.Tasks;

namespace Brighid.Discord
{
    /// <summary>
    /// Controller that handles an incoming event/message.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to handle.</typeparam>
    public interface IEventController<in TEvent>
        where TEvent : IGatewayEvent
    {
        /// <summary>
        /// Handles the incoming <paramref name="event"/>.
        /// </summary>
        /// <param name="event">The incoming event to handle.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        Task Handle(TEvent @event, CancellationToken cancellationToken);
    }
}
