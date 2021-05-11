using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Gateway;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

namespace Brighid.Discord.Adapter.Events
{
    public class ReadyEventControllerTests
    {
        [TestFixture]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] ReadyEventController controller
            )
            {
                var cancellationToken = new CancellationToken(true);
                var @event = new ReadyEvent { SessionId = sessionId };

                Func<Task> func = () => controller.Handle(@event, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
                gateway.DidNotReceive().SessionId = sessionId;
            }

            [Test, Auto]
            public async Task ShouldSetTheGatewayServiceSessionId(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] ReadyEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new ReadyEvent { SessionId = sessionId };

                await controller.Handle(@event, cancellationToken);

                gateway.Received().SessionId = sessionId;
            }

            [Test, Auto]
            public async Task ShouldSetTheGatewayServiceReadyPropertyToTrue(
                string sessionId,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] ReadyEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new ReadyEvent { SessionId = sessionId };

                await controller.Handle(@event, cancellationToken);

                gateway.Received().IsReady = true;
            }
        }
    }
}
