using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;

using Brighid.Discord.Gateway;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Events
{
    public class HelloEventTests
    {
        [TestFixture]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                [Substitute] IGatewayService gateway,
                uint interval
            )
            {
                var cancellationToken = new CancellationToken(true);
                var @event = new HelloEvent { HeartbeatInterval = interval };

                Func<Task> func = () => @event.Handle(gateway, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
                gateway.DidNotReceive().StartHeartbeat(Any<uint>());
            }

            [Test, Auto]
            public async Task ShouldStartHeartbeat(
                [Substitute] IGatewayService gateway,
                uint interval
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new HelloEvent { HeartbeatInterval = interval };

                await @event.Handle(gateway, cancellationToken);

                gateway.Received().StartHeartbeat(interval);
            }
        }
    }
}
