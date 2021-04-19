using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Gateway;
using Brighid.Discord.Messages;
using Brighid.Discord.Metrics;
using Brighid.Discord.Models;

using FluentAssertions;

using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Events
{
    public class HelloEventControllerTests
    {
        [TestFixture]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                uint interval,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] HelloEventController controller
            )
            {
                var cancellationToken = new CancellationToken(true);
                var @event = new HelloEvent { HeartbeatInterval = interval };

                Func<Task> func = () => controller.Handle(@event, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
                gateway.DidNotReceive().StartHeartbeat(Any<uint>());
            }

            [Test, Auto]
            public async Task ShouldStartHeartbeat(
                uint interval,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] HelloEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new HelloEvent { HeartbeatInterval = interval };

                await controller.Handle(@event, cancellationToken);

                gateway.Received().StartHeartbeat(interval);
            }

            [Test, Auto]
            public async Task ShouldSendIdentifyOpCode(
                string token,
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] HelloEventController controller
            )
            {
                options.Value.Token = token;
                var cancellationToken = new CancellationToken(false);
                var @event = new HelloEvent { };
                await controller.Handle(@event, cancellationToken);

                await gateway.ReceivedWithAnyArgs().Send(default, default);

                var message = (GatewayMessage)gateway.ReceivedCalls().ElementAt(1).GetArguments()[0];
                message.OpCode.Should().Be(GatewayOpCode.Identify);
            }

            [Test, Auto]
            public async Task ShouldSendIdentifyWithToken(
                string token,
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] HelloEventController controller
            )
            {
                options.Value.Token = token;
                var cancellationToken = new CancellationToken(false);
                var @event = new HelloEvent { };
                await controller.Handle(@event, cancellationToken);

                await gateway.ReceivedWithAnyArgs().Send(default, default);

                var message = (GatewayMessage)gateway.ReceivedCalls().ElementAt(1).GetArguments()[0];
                var identifyEvent = (IdentifyEvent)message.Data!;

                identifyEvent.Token.Should().Be(options.Value.Token);
            }

            [Test, Auto]
            public async Task ShouldSendIdentifyWithConnectionProperties(
                string libraryName,
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] HelloEventController controller
            )
            {
                options.Value.LibraryName = libraryName;
                var cancellationToken = new CancellationToken(false);
                var @event = new HelloEvent { };
                await controller.Handle(@event, cancellationToken);

                await gateway.ReceivedWithAnyArgs().Send(default, default);

                var message = (GatewayMessage)gateway.ReceivedCalls().ElementAt(1).GetArguments()[0];
                var identifyEvent = (IdentifyEvent)message.Data!;

                identifyEvent.ConnectionProperties.OperatingSystem.Should().Be(Environment.OSVersion.Platform.ToString());
                identifyEvent.ConnectionProperties.Browser.Should().Be(libraryName);
                identifyEvent.ConnectionProperties.Device.Should().Be(libraryName);
            }

            [Test, Auto]
            public async Task ShouldSendIdentifyWithIntents(
                string libraryName,
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] HelloEventController controller
            )
            {
                options.Value.LibraryName = libraryName;
                var cancellationToken = new CancellationToken(false);
                var @event = new HelloEvent { };
                await controller.Handle(@event, cancellationToken);

                await gateway.ReceivedWithAnyArgs().Send(default, default);

                var message = (GatewayMessage)gateway.ReceivedCalls().ElementAt(1).GetArguments()[0];
                var identifyEvent = (IdentifyEvent)message.Data!;

                identifyEvent.Intents.Should().HaveFlag(Intent.Guilds);
                identifyEvent.Intents.Should().HaveFlag(Intent.GuildMessages);
                identifyEvent.Intents.Should().HaveFlag(Intent.DirectMessages);
            }

            [Test, Auto]
            public async Task ShouldReportHelloEventMetric(
                [Frozen, Substitute] IMetricReporter reporter,
                [Target] HelloEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                await controller.Handle(new HelloEvent { }, cancellationToken);

                await reporter.Received().Report(Is(default(HelloEventMetric)), Is(cancellationToken));
            }
        }
    }
}
