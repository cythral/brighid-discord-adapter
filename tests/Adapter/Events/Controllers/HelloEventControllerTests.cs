using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Gateway;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Models;

using FluentAssertions;

using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Events
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
                await gateway.DidNotReceive().StartHeartbeat(Any<uint>());
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

                await gateway.Received().StartHeartbeat(interval);
            }

            [Test, Auto]
            public async Task ShouldResumeIfBothSessionIdAndSequenceNumberAreNotNull(
                string sessionId,
                int sequenceNumber,
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] HelloEventController controller
            )
            {
                gateway.SessionId = sessionId;
                gateway.SequenceNumber = sequenceNumber;

                var cancellationToken = new CancellationToken(false);
                var @event = new HelloEvent { };
                await controller.Handle(@event, cancellationToken);

                await gateway.ReceivedWithAnyArgs().Send(default, default);

                var message = (from call in gateway.ReceivedCalls()
                               where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                               select (GatewayMessage)call.GetArguments()[0]).First();

                message.Data.Should().BeOfType<ResumeEvent>();
            }

            [Test, Auto]
            public async Task ShouldIdentifyIfSessionIdIsNullButSequenceNumberIsNot(
                int sequenceNumber,
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] HelloEventController controller
            )
            {
                gateway.SessionId = null;
                gateway.SequenceNumber = sequenceNumber;

                var cancellationToken = new CancellationToken(false);
                var @event = new HelloEvent { };
                await controller.Handle(@event, cancellationToken);

                await gateway.ReceivedWithAnyArgs().Send(default, default);

                var message = (from call in gateway.ReceivedCalls()
                               where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                               select (GatewayMessage)call.GetArguments()[0]).First();

                message.Data.Should().BeOfType<IdentifyEvent>();
            }

            [Test, Auto]
            public async Task ShouldIdentifyIfSequenceNumberIsNullButSessionIdIsNot(
                string sessionId,
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] HelloEventController controller
            )
            {
                gateway.SessionId = sessionId;
                gateway.SequenceNumber = null;

                var cancellationToken = new CancellationToken(false);
                var @event = new HelloEvent { };
                await controller.Handle(@event, cancellationToken);

                await gateway.ReceivedWithAnyArgs().Send(default, default);

                var message = (from call in gateway.ReceivedCalls()
                               where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                               select (GatewayMessage)call.GetArguments()[0]).First();

                message.Data.Should().BeOfType<IdentifyEvent>();
            }

            [Test, Auto]
            public async Task ShouldIdentifyIfBothSequenceNumberAndSessionIdAreNull(
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] HelloEventController controller
            )
            {
                gateway.SessionId = null;
                gateway.SequenceNumber = null;

                var cancellationToken = new CancellationToken(false);
                var @event = new HelloEvent { };
                await controller.Handle(@event, cancellationToken);

                await gateway.ReceivedWithAnyArgs().Send(default, default);

                var message = (from call in gateway.ReceivedCalls()
                               where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                               select (GatewayMessage)call.GetArguments()[0]).First();

                message.Data.Should().BeOfType<IdentifyEvent>();
            }

            [TestFixture]
            public class ResumeFlow
            {
                [Test, Auto]
                public async Task ShouldSendResumeOpCode(
                    string token,
                    string sessionId,
                    int sequenceNumber,
                    [Frozen, Options] IOptions<AdapterOptions> adapterOptions,
                    [Frozen, Options] IOptions<GatewayOptions> gatewayOptions,
                    [Frozen, Substitute] IGatewayService gateway,
                    [Target] HelloEventController controller
                )
                {
                    adapterOptions.Value.Token = token;
                    gateway.SessionId = sessionId;
                    gateway.SequenceNumber = sequenceNumber;

                    var cancellationToken = new CancellationToken(false);
                    var @event = new HelloEvent { };
                    await controller.Handle(@event, cancellationToken);

                    await gateway.ReceivedWithAnyArgs().Send(default, default);

                    var message = (from call in gateway.ReceivedCalls()
                                   where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                                   select (GatewayMessage)call.GetArguments()[0]).First();

                    message.OpCode.Should().Be(GatewayOpCode.Resume);
                }

                [Test, Auto]
                public async Task ShouldSendResumeWithToken(
                    string token,
                    string sessionId,
                    int sequenceNumber,
                    [Frozen, Options] IOptions<AdapterOptions> options,
                    [Frozen, Substitute] IGatewayService gateway,
                    [Target] HelloEventController controller
                )
                {
                    options.Value.Token = token;
                    gateway.SessionId = sessionId;
                    gateway.SequenceNumber = sequenceNumber;

                    var cancellationToken = new CancellationToken(false);
                    var @event = new HelloEvent { };
                    await controller.Handle(@event, cancellationToken);

                    await gateway.ReceivedWithAnyArgs().Send(default, default);

                    var message = (from call in gateway.ReceivedCalls()
                                   where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                                   select (GatewayMessage)call.GetArguments()[0]).First();

                    var resumeEvent = (ResumeEvent)message.Data!;
                    resumeEvent.Token.Should().Be(token);
                }

                [Test, Auto]
                public async Task ShouldSendResumeWithGatewaySessionId(
                    string token,
                    string sessionId,
                    int sequenceNumber,
                    [Frozen, Options] IOptions<AdapterOptions> options,
                    [Frozen, Substitute] IGatewayService gateway,
                    [Target] HelloEventController controller
                )
                {
                    options.Value.Token = token;
                    gateway.SessionId = sessionId;
                    gateway.SequenceNumber = sequenceNumber;

                    var cancellationToken = new CancellationToken(false);
                    var @event = new HelloEvent { };
                    await controller.Handle(@event, cancellationToken);

                    await gateway.ReceivedWithAnyArgs().Send(default, default);

                    var message = (from call in gateway.ReceivedCalls()
                                   where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                                   select (GatewayMessage)call.GetArguments()[0]).First();

                    var resumeEvent = (ResumeEvent)message.Data!;
                    resumeEvent.SessionId.Should().Be(sessionId);
                }

                [Test, Auto]
                public async Task ShouldSendResumeWithGatewaySequenceNumber(
                    string token,
                    string sessionId,
                    int sequenceNumber,
                    [Frozen, Options] IOptions<AdapterOptions> options,
                    [Frozen, Substitute] IGatewayService gateway,
                    [Target] HelloEventController controller
                )
                {
                    options.Value.Token = token;
                    gateway.SessionId = sessionId;
                    gateway.SequenceNumber = sequenceNumber;

                    var cancellationToken = new CancellationToken(false);
                    var @event = new HelloEvent { };
                    await controller.Handle(@event, cancellationToken);

                    await gateway.ReceivedWithAnyArgs().Send(default, default);

                    var message = (from call in gateway.ReceivedCalls()
                                   where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                                   select (GatewayMessage)call.GetArguments()[0]).First();

                    var resumeEvent = (ResumeEvent)message.Data!;
                    resumeEvent.SequenceNumber.Should().Be(sequenceNumber);
                }
            }

            [TestFixture]
            public class IdentifyFlow
            {
                [Test, Auto]
                public async Task ShouldSendIdentifyOpCode(
                    string token,
                    [Frozen, Options] IOptions<AdapterOptions> options,
                    [Frozen, Substitute] IGatewayService gateway,
                    [Target] HelloEventController controller
                )
                {
                    options.Value.Token = token;
                    gateway.SessionId = null;

                    var cancellationToken = new CancellationToken(false);
                    var @event = new HelloEvent { };
                    await controller.Handle(@event, cancellationToken);

                    await gateway.ReceivedWithAnyArgs().Send(default, default);

                    var message = (from call in gateway.ReceivedCalls()
                                   where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                                   select (GatewayMessage)call.GetArguments()[0]).First();

                    message.OpCode.Should().Be(GatewayOpCode.Identify);
                }

                [Test, Auto]
                public async Task ShouldSendIdentifyWithToken(
                    string token,
                    [Frozen, Options] IOptions<AdapterOptions> options,
                    [Frozen, Substitute] IGatewayService gateway,
                    [Target] HelloEventController controller
                )
                {
                    options.Value.Token = token;
                    gateway.SessionId = null;

                    var cancellationToken = new CancellationToken(false);
                    var @event = new HelloEvent { };
                    await controller.Handle(@event, cancellationToken);

                    await gateway.ReceivedWithAnyArgs().Send(default, default);

                    var message = (from call in gateway.ReceivedCalls()
                                   where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                                   select (GatewayMessage)call.GetArguments()[0]).First();

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
                    gateway.SessionId = null;

                    var cancellationToken = new CancellationToken(false);
                    var @event = new HelloEvent { };
                    await controller.Handle(@event, cancellationToken);

                    await gateway.ReceivedWithAnyArgs().Send(default, default);

                    var message = (from call in gateway.ReceivedCalls()
                                   where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                                   select (GatewayMessage)call.GetArguments()[0]).First();

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
                    gateway.SessionId = null;

                    var cancellationToken = new CancellationToken(false);
                    var @event = new HelloEvent { };
                    await controller.Handle(@event, cancellationToken);

                    await gateway.ReceivedWithAnyArgs().Send(default, default);

                    var message = (from call in gateway.ReceivedCalls()
                                   where call.GetMethodInfo().Name == nameof(IGatewayService.Send)
                                   select (GatewayMessage)call.GetArguments()[0]).First();

                    var identifyEvent = (IdentifyEvent)message.Data!;

                    identifyEvent.Intents.Should().HaveFlag(Intent.Guilds);
                    identifyEvent.Intents.Should().HaveFlag(Intent.GuildMessages);
                    identifyEvent.Intents.Should().HaveFlag(Intent.DirectMessages);
                }
            }
        }
    }
}
