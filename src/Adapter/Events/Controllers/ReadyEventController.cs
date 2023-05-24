using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Gateway;
using Brighid.Discord.Adapter.Management;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Controller for the Hello Event.
    /// </summary>
    [EventController(typeof(ReadyEvent))]
    public class ReadyEventController : IEventController<ReadyEvent>
    {
        private readonly IGatewayService gateway;
        private readonly IGatewayMetadataService metadataService;
        private readonly ITrafficShifter shifter;
        private readonly ILogger<ReadyEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadyEventController" /> class.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="metadataService">Service for managing gateway metadata.</param>
        /// <param name="shifter">Service used for shifting traffic from previous shards to the current gateway.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public ReadyEventController(
            IGatewayService gateway,
            IGatewayMetadataService metadataService,
            ITrafficShifter shifter,
            ILogger<ReadyEventController> logger
        )
        {
            this.gateway = gateway;
            this.metadataService = metadataService;
            this.shifter = shifter;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(ReadyEvent @event, CancellationToken cancellationToken)
        {
            using var scope = logger.BeginScope("{@Event}", nameof(ReadyEvent));
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogInformation(LogEvents.ReadyEvent, "Received ready event from gateway.");
            gateway.SessionId = @event.SessionId;
            gateway.BotId = @event.User.Id;

            metadataService.SetGatewayUrl(new Uri(@event.ResumeGatewayUrl));
            await shifter.PerformTrafficShift(cancellationToken);
            gateway.SetReadyState(true);
        }
    }
}
