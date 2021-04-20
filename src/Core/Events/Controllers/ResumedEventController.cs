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
    [EventController(typeof(ResumedEvent))]
    public class ResumedEventController : IEventController<ResumedEvent>
    {
        private readonly IGatewayService gateway;
        private readonly IMetricReporter reporter;
        private readonly ILogger<ResumedEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResumedEventController" /> class.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="reporter">Reporter used to report metric.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public ResumedEventController(
            IGatewayService gateway,
            IMetricReporter reporter,
            ILogger<ResumedEventController> logger
        )
        {
            this.gateway = gateway;
            this.reporter = reporter;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(ResumedEvent @event, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            using var scope = logger.BeginScope("{@Event}", nameof(ResumedEvent));
            cancellationToken.ThrowIfCancellationRequested();

            _ = reporter.Report(default(ResumedEventMetric), cancellationToken);
            gateway.IsReady = true;
        }
    }
}
