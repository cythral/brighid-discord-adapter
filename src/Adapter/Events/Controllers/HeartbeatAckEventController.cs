using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Gateway;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Controller for the Hello Event.
    /// </summary>
    [EventController(typeof(HeartbeatAckEvent))]
    public class HeartbeatAckEventController : IEventController<HeartbeatAckEvent>
    {
        private readonly IGatewayService gateway;
        private readonly ILogger<HeartbeatAckEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartbeatAckEventController"/> class.
        /// </summary>
        /// <param name="gateway">Service for interacting with the gateway.</param>
        /// <param name="logger">Service used for logging messages.</param>
        public HeartbeatAckEventController(
            IGatewayService gateway,
            ILogger<HeartbeatAckEventController> logger
        )
        {
            this.gateway = gateway;
            this.logger = logger;
        }

        /// <inheritdoc />
        public Task Handle(HeartbeatAckEvent @event, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            gateway.AwaitingHeartbeatAcknowledgement = false;
            logger.LogInformation("Heartbeat acknowledged.");
            return Task.CompletedTask;
        }
    }
}
