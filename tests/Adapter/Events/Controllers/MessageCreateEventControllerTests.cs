using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Gateway;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Adapter.Metrics;
using Brighid.Discord.Adapter.Users;
using Brighid.Discord.Models;
using Brighid.Discord.RestClient.Client;

using FluentAssertions;

using Microsoft.Extensions.Localization;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Events
{
    public class MessageCreateEventControllerTests
    {
        [TestFixture]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                string content,
                [Frozen, Substitute] IMessageEmitter emitter,
                [Target] MessageCreateEventController controller
            )
            {
                var cancellationToken = new CancellationToken(true);
                var message = new Message { Content = content };
                var @event = new MessageCreateEvent { Message = message };

                Func<Task> func = () => controller.Handle(@event, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
                await emitter.DidNotReceive().Emit(Any<Message>(), Any<Snowflake>(), Any<CancellationToken>());
            }

            [Test, Auto]
            public async Task ShouldEmitMessageIfUserIsRegistered(
                string content,
                Snowflake userId,
                Snowflake channelId,
                [Frozen, Substitute] IMessageEmitter emitter,
                [Frozen, Substitute] IUserService userService,
                [Target] MessageCreateEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var author = new User { Id = userId };
                var message = new Message { Content = content, Author = author, ChannelId = channelId };
                var @event = new MessageCreateEvent { Message = message };

                userService.IsUserRegistered(Any<User>(), Any<CancellationToken>()).Returns(true);

                await controller.Handle(@event, cancellationToken);

                await emitter.Received().Emit(Is(message), Is(channelId), Is(cancellationToken));
                await userService.Received().IsUserRegistered(Is(author), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotEmitMessageIfUserIsNotRegistered(
                string content,
                Snowflake userId,
                Snowflake channelId,
                [Frozen, Substitute] IMessageEmitter emitter,
                [Frozen, Substitute] IUserService userService,
                [Target] MessageCreateEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var author = new User { Id = userId };
                var message = new Message { Content = content, Author = author, ChannelId = channelId, Mentions = Array.Empty<UserMention>() };
                var @event = new MessageCreateEvent { Message = message };

                userService.IsUserRegistered(Any<User>(), Any<CancellationToken>()).Returns(false);

                await controller.Handle(@event, cancellationToken);

                await emitter.DidNotReceive().Emit(Is(message), Is(channelId), Is(cancellationToken));
                await userService.Received().IsUserRegistered(Is(author), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldSendGreetingToUserViaDmsIfNotRegisteredAndWasMentioned(
                string content,
                Snowflake userId,
                Snowflake botId,
                Snowflake channelId,
                [Frozen] Channel channel,
                [Frozen] IdentityOptions options,
                [Frozen] IStringLocalizer<Strings> strings,
                [Frozen, Substitute] IDiscordUserClient userClient,
                [Frozen, Substitute] IDiscordChannelClient channelClient,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] IMessageEmitter emitter,
                [Frozen, Substitute] IUserService userService,
                [Target] MessageCreateEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var author = new User { Id = userId };
                var mention = new UserMention { Id = botId };
                var message = new Message { Content = content, Author = author, ChannelId = channelId, Mentions = new[] { mention } };
                var @event = new MessageCreateEvent { Message = message };

                gateway.BotId = botId;
                userService.IsUserRegistered(Any<User>(), Any<CancellationToken>()).Returns(false);

                await controller.Handle(@event, cancellationToken);

                await userClient.Received().CreateDirectMessageChannel(Is(userId), Is(cancellationToken));
                await channelClient.Received().CreateMessage(Is(channel.Id), Is<string>(strings["RegistrationGreeting", options.IdentityServerUri]!), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotSendGreetingToUserViaDmsIfNotRegisteredAndWasNotMentioned(
                string content,
                Snowflake userId,
                Snowflake botId,
                Snowflake channelId,
                [Frozen] Channel channel,
                [Frozen] IdentityOptions options,
                [Frozen] IStringLocalizer<Strings> strings,
                [Frozen, Substitute] IDiscordUserClient userClient,
                [Frozen, Substitute] IDiscordChannelClient channelClient,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] IMessageEmitter emitter,
                [Frozen, Substitute] IUserService userService,
                [Target] MessageCreateEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var author = new User { Id = userId };
                var message = new Message { Content = content, Author = author, ChannelId = channelId, Mentions = Array.Empty<UserMention>() };
                var @event = new MessageCreateEvent { Message = message };

                gateway.BotId = botId;
                userService.IsUserRegistered(Any<User>(), Any<CancellationToken>()).Returns(false);

                await controller.Handle(@event, cancellationToken);

                await userClient.DidNotReceive().CreateDirectMessageChannel(Is(userId), Is(cancellationToken));
                await channelClient.DidNotReceive().CreateMessage(Is(channel.Id), Is<string>(strings["RegistrationGreeting", options.IdentityServerUri]!), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldReportMessageCreateEventMetric(
                [Frozen, Substitute] IMetricReporter reporter,
                [Target] MessageCreateEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                await controller.Handle(new MessageCreateEvent { }, cancellationToken);

                await reporter.Received().Report(Is(default(MessageCreateEventMetric)), Is(cancellationToken));
            }
        }
    }
}
