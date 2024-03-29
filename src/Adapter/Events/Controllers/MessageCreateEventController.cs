using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Client;
using Brighid.Discord.Adapter.Gateway;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Adapter.Users;
using Brighid.Discord.Models.Payloads;
using Brighid.Discord.RestClient.Client;
using Brighid.Discord.Tracing;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable IDE0011 // Relax required braces

namespace Brighid.Discord.Adapter.Events
{
    /// <summary>
    /// Controller for handling message create events.
    /// </summary>
    [EventController(typeof(MessageCreateEvent))]
    public class MessageCreateEventController : IEventController<MessageCreateEvent>
    {
        private readonly ITracingService tracingService;
        private readonly IUserService userService;
        private readonly IMessageEmitter emitter;
        private readonly IDiscordUserClient discordUserClient;
        private readonly IDiscordChannelClient discordChannelClient;
        private readonly IStringLocalizer<Strings> strings;
        private readonly AdapterOptions adapterOptions;
        private readonly IBrighidCommandsService commandsService;
        private readonly IGatewayService gateway;
        private readonly ILogger<MessageCreateEventController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCreateEventController" /> class.
        /// </summary>
        /// <param name="tracingService">Service for doing traces.</param>
        /// <param name="userService">Service to manage users with.</param>
        /// <param name="emitter">Emitter to emit messages to.</param>
        /// <param name="discordUserClient">Client used to send User API requests to Discord.</param>
        /// <param name="discordChannelClient">Client used to send Channel API requests to Discord.</param>
        /// <param name="strings">Localizer service for retrieving strings.</param>
        /// <param name="adapterOptions">Options to use for the adapter.</param>
        /// <param name="commandsService">Client for interacting with the commands service.</param>
        /// <param name="gateway">Gateway that discord sends events through.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public MessageCreateEventController(
            ITracingService tracingService,
            IUserService userService,
            IMessageEmitter emitter,
            IDiscordUserClient discordUserClient,
            IDiscordChannelClient discordChannelClient,
            IStringLocalizer<Strings> strings,
            IOptions<AdapterOptions> adapterOptions,
            IBrighidCommandsService commandsService,
            IGatewayService gateway,
            ILogger<MessageCreateEventController> logger
        )
        {
            this.tracingService = tracingService;
            this.userService = userService;
            this.emitter = emitter;
            this.discordUserClient = discordUserClient;
            this.discordChannelClient = discordChannelClient;
            this.strings = strings;
            this.adapterOptions = adapterOptions.Value;
            this.commandsService = commandsService;
            this.gateway = gateway;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(MessageCreateEvent @event, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            gateway.ThrowIfNotReady();
            if (@event.IsBotMessage) return;

            using var trace = tracingService.StartTrace();
            using var scope = logger.BeginScope("{@Event} {@TraceId}", nameof(MessageCreateEvent), trace.Id);

            tracingService.AddAnnotation("event", "incoming-message");
            tracingService.AddAnnotation("messageId", @event.Message.Id);

            if (await userService.IsUserRegistered(@event.Message.Author, cancellationToken))
            {
                await HandleMessageFromRegisteredUser(@event, trace, cancellationToken);
                return;
            }

            if (@event.Message.Mentions.Any(mention => mention.Id == gateway.BotId))
            {
                await PerformWelcome(@event, cancellationToken);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Models.Embed[]? GetDebugEmbeds(UserId userId, TraceContext trace, ExecuteCommandResponse response)
        {
            return !userId.Debug
                ? null
                : new Models.Embed[]
                {
                    new()
                    {
                        Title = "Debug Info",
                        Color = 5763719,
                        Fields = new[]
                        {
                            new Models.EmbedField { Name = "TraceId", Value = trace.Id },
                            new Models.EmbedField { Name = "CommandVersion", Value = response.Version },
                        },
                    },
                };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task HandleMessageFromRegisteredUser(MessageCreateEvent @event, TraceContext trace, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = await userService.GetIdentityServiceUserId(@event.Message.Author, cancellationToken);
            if (!user.Enabled)
            {
                return;
            }

            logger.LogInformation("Message author is registered, parsing for possible command & emitting message.");
            await Task.WhenAll(new[]
            {
                EmitMessage(@event, cancellationToken),
                HandleCommand(@event, user, trace, cancellationToken),
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task EmitMessage(MessageCreateEvent @event, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await emitter.Emit(@event.Message, @event.Message.ChannelId, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task HandleCommand(MessageCreateEvent @event, UserId user, TraceContext trace, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await commandsService.ParseAndExecuteCommandAsUser(
                    message: @event.Message.Content,
                    userId: user.Id.ToString(),
                    sourceSystemChannel: @event.Message.ChannelId,
                    sourceSystemUser: @event.Message.Author.Id,
                    cancellationToken: cancellationToken
                );

                logger.LogInformation("Got execute command result: {@result}", result);

                if (result?.ReplyImmediately == true)
                {
                    var createMessagePayload = new CreateMessagePayload
                    {
                        Content = result.Response,
                        Embeds = GetDebugEmbeds(user, trace, result),
                    };

                    await discordChannelClient.CreateMessage(@event.Message.ChannelId, createMessagePayload, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Received exception while trying to execute a command");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task PerformWelcome(MessageCreateEvent @event, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                logger.LogInformation("User {@authorId} is not registered, sending invite through direct messages.", @event.Message.Author.Id);
                var dmChannel = await discordUserClient.CreateDirectMessageChannel(@event.Message.Author.Id, cancellationToken);
                var message = (string)strings["RegistrationGreeting", adapterOptions.RegistrationUrl]!;
                var payload = new CreateMessagePayload { Content = message };
                await discordChannelClient.CreateMessage(dmChannel.Id, payload, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError("An error occured while attempting to direct message {@authorId}: {@exception}", @event.Message.Author.Id, exception);
            }
        }
    }
}
