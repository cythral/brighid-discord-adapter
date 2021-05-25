using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.Serialization;
using Brighid.Discord.Threading;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.RestClient.Responses
{
    public class DefaultResponseServerTests
    {
        [TestFixture]
        public class StartAsyncTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                [Target] DefaultResponseServer server
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => server.StartAsync(cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCanceled(
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => server.StartAsync(cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldCreateATimer(
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                await server.StartAsync(cancellationToken);

                timerFactory.Received().CreateTimer(Is<AsyncTimerCallback>(server.Run), Is(0), Is("Response Server"));
            }

            [Test, Auto]
            public async Task ShouldStartTheListener(
                [Frozen, Substitute] ITcpListener listener,
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                await server.StartAsync(cancellationToken);

                listener.Received().Start();
            }

            [Test, Auto]
            public async Task ShouldStartTheTimer(
                [Frozen, Substitute] ITimer timer,
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                await server.StartAsync(cancellationToken);

                await timer.Received().Start();
            }
        }

        [TestFixture]
        public class StopAsyncTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                [Target] DefaultResponseServer server
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => server.StopAsync(cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCanceled(
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => server.StopAsync(cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldThrowIfNotRunning(
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => server.StopAsync(cancellationToken);

                await func.Should().ThrowAsync<InvalidOperationException>();
            }

            [Test, Auto]
            public async Task ShouldStopTheTimer(
                [Frozen, Substitute] ITimer timer,
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                await server.StartAsync(cancellationToken);
                await server.StopAsync(cancellationToken);

                await timer.Received().Stop();
            }

            [Test, Auto]
            public async Task ShouldStopTheListenerBeforeStoppingTheTimer(
                [Frozen, Substitute] ITcpListener listener,
                [Frozen, Substitute] ITimer timer,
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                await server.StartAsync(cancellationToken);
                await server.StopAsync(cancellationToken);

                Received.InOrder(async () =>
                {
                    listener.Received().Stop();
                    await timer.Received().Stop();
                });
            }
        }

        [TestFixture]
        public class RunTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                [Target] DefaultResponseServer server
            )
            {
                var cancellationToken = new CancellationToken(true);

                Func<Task> func = () => server.Run(cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCanceled(
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => server.Run(cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldAcceptATcpClient(
                [Frozen, Substitute] ITcpListener listener,
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                await server.Run(cancellationToken);

                await listener.Received().Accept();
            }

            [Test, Auto]
            public async Task ShouldDeserializeTheClientStreamToResponse(
                [Frozen] Stream stream,
                [Frozen, Substitute] ISerializer serializer,
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                await server.Run(cancellationToken);

                await serializer.Received().Deserialize<Response>(Is(stream), Is(cancellationToken));
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldCompleteAnyPendingRequests(
                Guid requestId,
                TaskCompletionSource<Response> promise,
                Response response,
                [Frozen, Substitute] IRequestMap requestMap,
                [Frozen, Substitute] ISerializer serializer,
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                response.RequestId = requestId;
                serializer.Deserialize<Response>(Any<Stream>(), Any<CancellationToken>()).Returns(response);
                requestMap.TryGetValue(Any<Guid>(), out Any<TaskCompletionSource<Response>?>()).Returns(x =>
                {
                    x[1] = promise;
                    return true;
                });

                await server.Run(cancellationToken);

                var result = await promise.Task;
                result.Should().Be(response);
            }
        }

        [TestFixture]
        public class ListenForResponseTests
        {
            [Test, Auto]
            public void ShouldAddTheRequestIdAndPromiseToTheRequestMap(
                Guid requestId,
                TaskCompletionSource<Response> promise,
                [Frozen, Substitute] IRequestMap map,
                [Target] DefaultResponseServer server,
                CancellationToken cancellationToken
            )
            {
                server.ListenForResponse(requestId, promise);

                map.Received().Add(Is(requestId), Is(promise));
            }
        }
    }
}
