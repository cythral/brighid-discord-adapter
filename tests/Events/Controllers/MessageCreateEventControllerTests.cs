using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Messages;
using Brighid.Discord.Metrics;
using Brighid.Discord.Models;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Events
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
            public async Task ShouldEmitMessage(
                string content,
                [Frozen, Substitute] IMessageEmitter emitter,
                [Target] MessageCreateEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var message = new Message { Content = content };
                var @event = new MessageCreateEvent { Message = message };

                await controller.Handle(@event, cancellationToken);

                await emitter.Received().Emit(Is(message), Is(cancellationToken));
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
