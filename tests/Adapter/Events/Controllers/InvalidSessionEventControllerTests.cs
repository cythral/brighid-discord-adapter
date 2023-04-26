using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Gateway;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Events
{
    public class InvalidSessionEventControllerTests
    {
        [TestFixture]
        [Category("Unit")]
        public class HandleTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                bool resumable,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] InvalidSessionEventController controller
            )
            {
                var cancellationToken = new CancellationToken(true);
                var @event = new InvalidSessionEvent(resumable);

                Func<Task> func = () => controller.Handle(@event, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
                await gateway.DidNotReceive().Restart();
            }

            [Test, Auto]
            public async Task ShouldSetGatewayUrlToNullIfNotResumable(
                [Frozen, Substitute] IGatewayMetadataService metadataService,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] InvalidSessionEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new InvalidSessionEvent(false);

                await controller.Handle(@event, cancellationToken);

                metadataService.Received().SetGatewayUrl(Is(null as Uri));
            }

            [Test, Auto]
            public async Task ShouldRestartTheGatewayServiceWithIsResumable(
                bool resumable,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] InvalidSessionEventController controller
            )
            {
                var cancellationToken = new CancellationToken(false);
                var @event = new InvalidSessionEvent(resumable);

                await controller.Handle(@event, cancellationToken);

                await gateway.Received().Restart(Is(resumable));
            }
        }
    }
}
