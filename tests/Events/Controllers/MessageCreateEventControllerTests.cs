using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.GatewayAdapter.Messages;
using Brighid.Discord.GatewayAdapter.Metrics;
using Brighid.Discord.GatewayAdapter.Users;
using Brighid.Discord.Models;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.GatewayAdapter.Events
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
                await emitter.DidNotReceive().Emit(Any<Message>(), Any<CancellationToken>());
            }

            [Test, Auto]
            public async Task ShouldEmitMessageIfUserIsRegistered(
                string content,
                Snowflake userId,
                [Frozen, Substitute] IMessageEmitter emitter,
                [Frozen, Substitute] IUserService userService,
                [Target] MessageCreateEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var author = new User { Id = userId };
                var message = new Message { Content = content, Author = author };
                var @event = new MessageCreateEvent { Message = message };

                userService.IsUserRegistered(Any<User>(), Any<CancellationToken>()).Returns(true);

                await controller.Handle(@event, cancellationToken);

                await emitter.Received().Emit(Is(message), Is(cancellationToken));
                await userService.Received().IsUserRegistered(Is(author), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotEmitMessageIfUserIsNotRegistered(
                string content,
                Snowflake userId,
                [Frozen, Substitute] IMessageEmitter emitter,
                [Frozen, Substitute] IUserService userService,
                [Target] MessageCreateEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var author = new User { Id = userId };
                var message = new Message { Content = content, Author = author };
                var @event = new MessageCreateEvent { Message = message };

                userService.IsUserRegistered(Any<User>(), Any<CancellationToken>()).Returns(false);

                await controller.Handle(@event, cancellationToken);

                await emitter.DidNotReceive().Emit(Is(message), Is(cancellationToken));
                await userService.Received().IsUserRegistered(Is(author), Is(cancellationToken));
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
