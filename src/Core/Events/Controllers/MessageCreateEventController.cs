using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Messages;
using Brighid.Discord.Metrics;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Events
{
    /// <summary>
    /// Controller for handling message create events.
    /// </summary>
    [EventController(typeof(MessageCreateEvent))]
    public class MessageCreateEventController : IEventController<MessageCreateEvent>
    {
        private readonly IMessageEmitter emitter;
        private readonly IMetricReporter reporter;
        private readonly ILogger<MessageCreateEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCreateEventController" /> class.
        /// </summary>
        /// <param name="emitter">Emitter to emit messages to.</param>
        /// <param name="reporter">Reporter to report metrics to.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public MessageCreateEventController(
            IMessageEmitter emitter,
            IMetricReporter reporter,
            ILogger<MessageCreateEventController> logger
        )
        {
            this.emitter = emitter;
            this.reporter = reporter;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(MessageCreateEvent @event, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = logger.BeginScope("{@Event}", nameof(MessageCreateEvent));
            await emitter.Emit(@event.Message, cancellationToken);
            await reporter.Report(default(MessageCreateEventMetric), cancellationToken);
        }
    }
}
