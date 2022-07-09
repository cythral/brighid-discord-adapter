using System.Threading;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Gateway;

using FluentAssertions;

using NUnit.Framework;

namespace Brighid.Discord.Adapter.Events
{
    public class HeartbeatAckEventControllerTests
    {
        [TestFixture]
        [Category("Unit")]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldSetGatewayAwaitingHeartbeatToFalse(
                HeartbeatAckEvent @event,
                [Frozen] IGatewayService gateway,
                [Target] HeartbeatAckEventController controller,
                CancellationToken cancellationToken
            )
            {
                gateway.AwaitingHeartbeatAcknowledgement = true;

                await controller.Handle(@event, cancellationToken);

                gateway.AwaitingHeartbeatAcknowledgement.Should().BeFalse();
            }
        }
    }
}
