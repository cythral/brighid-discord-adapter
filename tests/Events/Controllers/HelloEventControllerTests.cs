using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Gateway;

using FluentAssertions;

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
        }
    }
}
