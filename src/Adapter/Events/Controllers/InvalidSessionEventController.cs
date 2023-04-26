using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Gateway;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Controller for the Hello Event.
    /// </summary>
    [EventController(typeof(InvalidSessionEvent))]
    public class InvalidSessionEventController : IEventController<InvalidSessionEvent>
    {
        private readonly IGatewayService gateway;
        private readonly IGatewayUtilsFactory utilsFactory;
        private readonly IGatewayMetadataService metadataService;
        private readonly ILogger<InvalidSessionEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSessionEventController" /> class.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="utilsFactory">Factory to create utilities with.</param>
        /// <param name="metadataService">Service for managing gateway metadata.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public InvalidSessionEventController(
            IGatewayService gateway,
            IGatewayUtilsFactory utilsFactory,
            IGatewayMetadataService metadataService,
            ILogger<InvalidSessionEventController> logger
        )
        {
            this.gateway = gateway;
            this.utilsFactory = utilsFactory;
            this.metadataService = metadataService;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(InvalidSessionEvent @event, CancellationToken cancellationToken)
        {
            using (var scope = logger.BeginScope("{@Event}", nameof(InvalidSessionEvent)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                logger.LogWarning(LogEvents.InvalidSessionEvent, "Received an invalid session event");

                if (!@event.IsResumable)
                {
                    metadataService.SetGatewayUrl(null);
                }
            }

            await gateway.Restart(@event.IsResumable, CancellationToken.None);
        }
    }
}
