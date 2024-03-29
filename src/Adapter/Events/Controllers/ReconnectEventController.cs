using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Gateway;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Controller for the Hello Event.
    /// </summary>
    [EventController(typeof(ReconnectEvent))]
    public class ReconnectEventController : IEventController<ReconnectEvent>
    {
        private readonly IGatewayService gateway;
        private readonly IGatewayUtilsFactory utilsFactory;
        private readonly ILogger<ReconnectEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconnectEventController" /> class.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="utilsFactory">Factory to create utilities with.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public ReconnectEventController(
            IGatewayService gateway,
            IGatewayUtilsFactory utilsFactory,
            ILogger<ReconnectEventController> logger
        )
        {
            this.gateway = gateway;
            this.utilsFactory = utilsFactory;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(ReconnectEvent @event, CancellationToken cancellationToken)
        {
            using (var scope = logger.BeginScope("{@Event}", nameof(ReconnectEvent)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var cancellationTokenSource = new CancellationTokenSource();

                logger.LogWarning(LogEvents.ReconnectEvent, "Received a reconnect event");
            }

            await gateway.Restart(cancellationToken: CancellationToken.None);
        }
    }
}
