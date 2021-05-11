using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Gateway;
using Brighid.Discord.Adapter.Metrics;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Events
{
    public class InvalidSessionEventControllerTests
    {
        [TestFixture]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                bool resumable,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] InvalidSessionEventController controller
            )
            {
                var cancellationToken = new CancellationToken(true);
                var @event = new InvalidSessionEvent(resumable);

                Func<Task> func = () => controller.Handle(@event, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
                gateway.DidNotReceive().Stop();
                gateway.DidNotReceive().Start(Any<CancellationTokenSource>());
            }

            [Test, Auto]
            public async Task ShouldRestartTheGatewayServiceWithIsResumable(
                bool resumable,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] InvalidSessionEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new InvalidSessionEvent(resumable);

                await controller.Handle(@event, cancellationToken);

                await gateway.Received().Restart(Is(resumable));
            }

            [Test, Auto]
            public async Task ShouldReportAReconnectMetric(
                bool resumable,
                [Frozen, Substitute] IMetricReporter reporter,
                [Target] InvalidSessionEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new InvalidSessionEvent(resumable);

                await controller.Handle(@event, cancellationToken);

                await reporter.Received().Report(Is(default(InvalidSessionEventMetric)), Is(cancellationToken));
            }
        }
    }
}
