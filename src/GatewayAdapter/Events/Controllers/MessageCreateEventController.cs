using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.GatewayAdapter.Messages;
using Brighid.Discord.GatewayAdapter.Metrics;
using Brighid.Discord.GatewayAdapter.Users;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.GatewayAdapter.Events
{
    /// <summary>
    /// Controller for handling message create events.
    /// </summary>
    [EventController(typeof(MessageCreateEvent))]
    public class MessageCreateEventController : IEventController<MessageCreateEvent>
    {
        private readonly IUserService userService;
        private readonly IMessageEmitter emitter;
        private readonly IMetricReporter reporter;
        private readonly ILogger<MessageCreateEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCreateEventController" /> class.
        /// </summary>
        /// <param name="userService">Service to manage users with.</param>
        /// <param name="emitter">Emitter to emit messages to.</param>
        /// <param name="reporter">Reporter to report metrics to.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public MessageCreateEventController(
            IUserService userService,
            IMessageEmitter emitter,
            IMetricReporter reporter,
            ILogger<MessageCreateEventController> logger
        )
        {
            this.userService = userService;
            this.emitter = emitter;
            this.reporter = reporter;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(MessageCreateEvent @event, CancellationToken cancellationToken)
        {
            using var scope = logger.BeginScope("{@Event}", nameof(MessageCreateEvent));
            cancellationToken.ThrowIfCancellationRequested();
            _ = reporter.Report(default(MessageCreateEventMetric), cancellationToken);

            if (await userService.IsUserRegistered(@event.Message.Author, cancellationToken))
            {
                logger.LogInformation("Message author is registered, emitting message.");
                await emitter.Emit(@event.Message, cancellationToken);
            }
        }
    }
}
