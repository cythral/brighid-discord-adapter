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
#pragma warning disable SA1005

namespace Brighid.Discord.Adapter.Events
{
    public class ResumedEventControllerTests
    {
        [TestFixture]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] ResumedEventController controller
            )
            {
                var cancellationToken = new CancellationToken(true);
                var @event = new ResumedEvent { };

                Func<Task> func = () => controller.Handle(@event, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
                gateway.DidNotReceive().IsReady = true;
            }

            [Test, Auto]
            public async Task ShouldSetTheGatewayIsReadyPropToTrue(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] ResumedEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new ResumedEvent { };

                await controller.Handle(@event, cancellationToken);

                gateway.Received().IsReady = true;
            }

            [Test, Auto]
            public async Task ShouldReportAResumedMetric(
                string sessionId,
                [Frozen, Substitute] IMetricReporter reporter,
                [Target] ResumedEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new ResumedEvent { };

                await controller.Handle(@event, cancellationToken);

                await reporter.Received().Report(Is(default(ResumedEventMetric)), Is(cancellationToken));
            }
        }
    }
}
