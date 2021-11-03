using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Client;
using Brighid.Discord.Adapter.Gateway;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Adapter.Metrics;
using Brighid.Discord.Adapter.Users;
using Brighid.Discord.RestClient.Client;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Controller for handling message create events.
    /// </summary>
    [EventController(typeof(MessageCreateEvent))]
    public class MessageCreateEventController : IEventController<MessageCreateEvent>
    {
        private readonly IUserService userService;
        private readonly IMessageEmitter emitter;
        private readonly IDiscordUserClient discordUserClient;
        private readonly IDiscordChannelClient discordChannelClient;
        private readonly IStringLocalizer<Strings> strings;
        private readonly AdapterOptions adapterOptions;
        private readonly IBrighidCommandsService commandsService;
        private readonly IGatewayService gateway;
        private readonly IMetricReporter reporter;
        private readonly ILogger<MessageCreateEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCreateEventController" /> class.
        /// </summary>
        /// <param name="userService">Service to manage users with.</param>
        /// <param name="emitter">Emitter to emit messages to.</param>
        /// <param name="discordUserClient">Client used to send User API requests to Discord.</param>
        /// <param name="discordChannelClient">Client used to send Channel API requests to Discord.</param>
        /// <param name="strings">Localizer service for retrieving strings.</param>
        /// <param name="adapterOptions">Options to use for the adapter.</param>
        /// <param name="commandsService">Client for interacting with the commands service.</param>
        /// <param name="gateway">Gateway that discord sends events through.</param>
        /// <param name="reporter">Reporter to report metrics to.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public MessageCreateEventController(
            IUserService userService,
            IMessageEmitter emitter,
            IDiscordUserClient discordUserClient,
            IDiscordChannelClient discordChannelClient,
            IStringLocalizer<Strings> strings,
            IOptions<AdapterOptions> adapterOptions,
            IBrighidCommandsService commandsService,
            IGatewayService gateway,
            IMetricReporter reporter,
            ILogger<MessageCreateEventController> logger
        )
        {
            this.userService = userService;
            this.emitter = emitter;
            this.discordUserClient = discordUserClient;
            this.discordChannelClient = discordChannelClient;
            this.strings = strings;
            this.adapterOptions = adapterOptions.Value;
            this.commandsService = commandsService;
            this.gateway = gateway;
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
                var userId = await userService.GetIdentityServiceUserId(@event.Message.Author, cancellationToken);
                var userIdString = userId.Id.ToString();

                if (userId.Enabled)
                {
                    logger.LogInformation("Message author is registered, parsing for possible command & emitting message.");
                    _ = emitter.Emit(@event.Message, @event.Message.ChannelId, cancellationToken);

                    var result = await commandsService.ParseAndExecuteCommandAsUser(@event.Message.Content, userIdString, cancellationToken);
                    logger.LogInformation("Got result: {@result}", result);
                    if (result?.ReplyImmediately == true)
                    {
                        await discordChannelClient.CreateMessage(@event.Message.ChannelId, result.Response, cancellationToken);
                    }
                }

                return;
            }

            if (@event.Message.Mentions.Any(mention => mention.Id == gateway.BotId))
            {
                var dmChannel = await discordUserClient.CreateDirectMessageChannel(@event.Message.Author.Id, cancellationToken);
                var message = (string)strings["RegistrationGreeting", adapterOptions.RegistrationUrl]!;
                await discordChannelClient.CreateMessage(dmChannel.Id, message, cancellationToken);
            }
        }
    }
}
