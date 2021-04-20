using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Gateway;
using Brighid.Discord.Metrics;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Events
{
    /// <summary>
    /// Controller for the Hello Event.
    /// </summary>
    [EventController(typeof(InvalidSessionEvent))]
    public class InvalidSessionEventController : IEventController<InvalidSessionEvent>
    {
        private readonly IGatewayService gateway;
        private readonly IGatewayUtilsFactory utilsFactory;
        private readonly IMetricReporter reporter;
        private readonly ILogger<InvalidSessionEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSessionEventController" /> class.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="utilsFactory">Factory to create utilities with.</param>
        /// <param name="reporter">Reporter used to report metrics.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public InvalidSessionEventController(
            IGatewayService gateway,
            IGatewayUtilsFactory utilsFactory,
            IMetricReporter reporter,
            ILogger<InvalidSessionEventController> logger
        )
        {
            this.gateway = gateway;
            this.utilsFactory = utilsFactory;
            this.logger = logger;
            this.reporter = reporter;
        }

        /// <inheritdoc />
        public async Task Handle(InvalidSessionEvent @event, CancellationToken cancellationToken)
        {
            using var scope = logger.BeginScope("{@Event}", nameof(InvalidSessionEvent));
            cancellationToken.ThrowIfCancellationRequested();
            var cancellationTokenSource = new CancellationTokenSource();

            _ = reporter.Report(default(InvalidSessionEventMetric), cancellationToken);

#pragma warning disable CA2016 // Forwarding the cancellationToken here would cause an issue during restart
            await gateway.Restart(@event.IsResumable);
#pragma warning restore CA2016
        }
    }
}
