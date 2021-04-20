using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Gateway;
using Brighid.Discord.Metrics;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Events
{
    public class ReconnectEventControllerTests
    {
        [TestFixture]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] ReconnectEventController controller
            )
            {
                var cancellationToken = new CancellationToken(true);
                var @event = new ReconnectEvent { };

                Func<Task> func = () => controller.Handle(@event, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
                gateway.DidNotReceive().Stop();
                gateway.DidNotReceive().Start(Any<CancellationTokenSource>());
            }

            [Test, Auto]
            public async Task ShouldRestartTheGatewayService(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] ReconnectEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new ReconnectEvent { };

                await controller.Handle(@event, cancellationToken);

                await gateway.Received().Restart();
            }

            [Test, Auto]
            public async Task ShouldReportAReconnectMetric(
                string sessionId,
                [Frozen, Substitute] IMetricReporter reporter,
                [Target] ReconnectEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new ReconnectEvent { };

                await controller.Handle(@event, cancellationToken);

                await reporter.Received().Report(Is(default(ReconnectEventMetric)), Is(cancellationToken));
            }
        }
    }
}
