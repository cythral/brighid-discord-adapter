using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.GatewayAdapter.Gateway;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.GatewayAdapter.Events
{
    /// <summary>
    /// Controller for the Hello Event.
    /// </summary>
    [EventController(typeof(ReadyEvent))]
    public class ReadyEventController : IEventController<ReadyEvent>
    {
        private readonly IGatewayService gateway;
        private readonly ILogger<ReadyEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadyEventController" /> class.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public ReadyEventController(
            IGatewayService gateway,
            ILogger<ReadyEventController> logger
        )
        {
            this.gateway = gateway;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(ReadyEvent @event, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            using var scope = logger.BeginScope("{@Event}", nameof(ReadyEvent));
            cancellationToken.ThrowIfCancellationRequested();

            gateway.SessionId = @event.SessionId;
            gateway.IsReady = true;
        }
    }
}
