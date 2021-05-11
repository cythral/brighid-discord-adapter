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

namespace Brighid.Discord.Events
{
    public class DefaultGatewayRestartServiceTests
    {
        [TestFixture]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRestartService service
            )
            {
                var cancellationToken = new CancellationToken(true);
                var resume = true;

                Func<Task> func = () => service.Restart(gateway, resume, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
                await gateway.DidNotReceive().StopAsync(Is(cancellationToken));
                await gateway.DidNotReceive().StartAsync(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldStopTheGatewayService(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRestartService service
            )
            {
                var cancellationToken = new CancellationToken(false);
                var resume = true;

                await service.Restart(gateway, resume, cancellationToken);

                await gateway.Received().StopAsync(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldSetSessionIdToNullIfNotResumable(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRestartService service
            )
            {
                var cancellationToken = new CancellationToken(false);
                var resume = false;

                await service.Restart(gateway, resume, cancellationToken);

                gateway.Received().SessionId = null;
            }

            [Test, Auto]
            public async Task ShouldNotSetSessionIdToNullIfResumable(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRestartService service
            )
            {
                var cancellationToken = new CancellationToken(false);
                var resume = true;

                await service.Restart(gateway, resume, cancellationToken);

                gateway.DidNotReceive().SessionId = null;
            }

            [Test, Auto]
            public async Task ShouldWaitRandomAmountOfTimeBetween1And5Seconds(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayRestartService service
            )
            {
                var cancellationToken = new CancellationToken(false);
                var resume = true;

                await service.Restart(gateway, resume, cancellationToken);

                await factory.Received().CreateRandomDelay(1000, 5000, Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldStartTheGateway(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRestartService service
            )
            {
                var cancellationToken = new CancellationToken(false);
                var resume = true;

                await service.Restart(gateway, resume, cancellationToken);

                await gateway.Received().StartAsync(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldReportARestartMetric(
                string sessionId,
                [Frozen, Substitute] IMetricReporter reporter,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRestartService service
            )
            {
                var cancellationToken = new CancellationToken(false);
                var resume = true;

                await service.Restart(gateway, resume, cancellationToken);

                await reporter.Received().Report(Is(default(GatewayRestartMetric)), Is(cancellationToken));
            }
        }
    }
}
