using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Gateway;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Controller for the Hello Event.
    /// </summary>
    [EventController(typeof(ResumedEvent))]
    public class ResumedEventController : IEventController<ResumedEvent>
    {
        private readonly IGatewayService gateway;
        private readonly ILogger<ResumedEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResumedEventController" /> class.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public ResumedEventController(
            IGatewayService gateway,
            ILogger<ResumedEventController> logger
        )
        {
            this.gateway = gateway;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(ResumedEvent @event, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            using var scope = logger.BeginScope("{@Event}", nameof(ResumedEvent));
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogInformation(LogEvents.ResumedEvent, "Received a resumed event.");
            gateway.SetReadyState(true);
        }
    }
}
