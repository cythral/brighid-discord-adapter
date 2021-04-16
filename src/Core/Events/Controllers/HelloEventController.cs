using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Gateway;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Events
{
    /// <summary>
    /// Controller for the Hello Event.
    /// </summary>
    [EventController(typeof(HelloEvent))]
    public class HelloEventController : IEventController<HelloEvent>
    {
        private readonly IGatewayService gateway;
        private readonly ILogger<HelloEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelloEventController" /> class.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public HelloEventController(
            IGatewayService gateway,
            ILogger<HelloEventController> logger
        )
        {
            this.gateway = gateway;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(HelloEvent @event, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling Hello Event");
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            gateway.StartHeartbeat(@event.HeartbeatInterval);
        }
    }
}
