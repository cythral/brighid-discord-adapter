using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Gateway;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Adapter.Metrics;
using Brighid.Discord.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Controller for the Hello Event.
    /// </summary>
    [EventController(typeof(HelloEvent))]
    public class HelloEventController : IEventController<HelloEvent>
    {
        private readonly IGatewayService gateway;
        private readonly AdapterOptions adapterOptions;
        private readonly GatewayOptions gatewayOptions;
        private readonly IMetricReporter reporter;
        private readonly ILogger<HelloEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelloEventController" /> class.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="reporter">Reporter to use to report metrics with.</param>
        /// <param name="adapterOptions">Options used across the adapter.</param>
        /// <param name="gatewayOptions">The options to use for the gateway.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public HelloEventController(
            IGatewayService gateway,
            IMetricReporter reporter,
            IOptions<AdapterOptions> adapterOptions,
            IOptions<GatewayOptions> gatewayOptions,
            ILogger<HelloEventController> logger
        )
        {
            this.gateway = gateway;
            this.reporter = reporter;
            this.adapterOptions = adapterOptions.Value;
            this.gatewayOptions = gatewayOptions.Value;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(HelloEvent @event, CancellationToken cancellationToken)
        {
            using var scope = logger.BeginScope("{@Event}", nameof(HelloEvent));
            cancellationToken.ThrowIfCancellationRequested();

            _ = reporter.Report(default(HelloEventMetric), cancellationToken);
            gateway.StartHeartbeat(@event.HeartbeatInterval);

            var message = gateway.SessionId == null || gateway.SequenceNumber == null ? CreateIdentifyMessage() : CreateResumeMessage();
            await gateway.Send(message, cancellationToken);
        }

        private GatewayMessage CreateIdentifyMessage()
        {
            return new GatewayMessage
            {
                OpCode = GatewayOpCode.Identify,
                Data = new IdentifyEvent
                {
                    Token = adapterOptions.Token,
                    Intents = Intent.Guilds | Intent.GuildMessages | Intent.DirectMessages,
                    ConnectionProperties = new()
                    {
                        OperatingSystem = Environment.OSVersion.Platform.ToString(),
                        Browser = gatewayOptions.LibraryName,
                        Device = gatewayOptions.LibraryName,
                    },
                },
            };
        }

        private GatewayMessage CreateResumeMessage()
        {
            return new GatewayMessage
            {
                OpCode = GatewayOpCode.Resume,
                Data = new ResumeEvent
                {
                    Token = adapterOptions.Token,
                    SessionId = gateway.SessionId!,
                    SequenceNumber = gateway.SequenceNumber,
                },
            };
        }
    }
}
