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
                await gateway.DidNotReceive().Restart();
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
        }
    }
}
