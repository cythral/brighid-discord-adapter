using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Gateway;
using Brighid.Discord.Messages;
using Brighid.Discord.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Events
{
    /// <summary>
    /// Controller for the Hello Event.
    /// </summary>
    [EventController(typeof(HelloEvent))]
    public class HelloEventController : IEventController<HelloEvent>
    {
        private readonly IGatewayService gateway;
        private readonly GatewayOptions options;
        private readonly ILogger<HelloEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelloEventController" /> class.
        /// </summary>
        /// <param name="gateway">The gateway service to use.</param>
        /// <param name="options">The options to use for the gateway.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public HelloEventController(
            IGatewayService gateway,
            IOptions<GatewayOptions> options,
            ILogger<HelloEventController> logger
        )
        {
            this.gateway = gateway;
            this.options = options.Value;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(HelloEvent @event, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling Hello Event");
            cancellationToken.ThrowIfCancellationRequested();

            gateway.StartHeartbeat(@event.HeartbeatInterval);

            var identifyEvent = new IdentifyEvent
            {
                Token = options.Token,
                Intents = Intent.Guilds | Intent.GuildMessages | Intent.DirectMessages,
                ConnectionProperties = new()
                {
                    OperatingSystem = Environment.OSVersion.Platform.ToString(),
                    Browser = options.LibraryName,
                    Device = options.LibraryName,
                },
            };

            var message = new GatewayMessage { OpCode = GatewayOpCode.Identify, Data = identifyEvent };
            await gateway.Send(message, cancellationToken);
        }
    }
}
