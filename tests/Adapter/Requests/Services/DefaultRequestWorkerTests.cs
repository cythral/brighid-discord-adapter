using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.NUnit3;

using Brighid.Discord.DependencyInjection;
using Brighid.Discord.Threading;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Requests
{
    public class DefaultRequestWorkerTests
    {
        [TestFixture]
        public class StartAsyncTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                [Target] DefaultRequestWorker worker
            )
            {
                var cancellationToken = new CancellationToken(true);
                Func<Task> func = () => worker.StartAsync(cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                [Target] DefaultRequestWorker worker,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => worker.StartAsync(cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldCreateAndStartANewTimer(
                [Frozen] RequestOptions options,
                [Frozen] ITimer timer,
                [Frozen] ITimerFactory timerFactory,
                [Target] DefaultRequestWorker worker,
                CancellationToken cancellationToken
            )
            {
                await worker.StartAsync(cancellationToken);

                timerFactory.Received().CreateTimer(Is<AsyncTimerCallback>(worker.Run), Is(options.PollingInterval), Is(nameof(DefaultRequestWorker)));
                await timer.Received().Start();
            }
        }

        [TestFixture]
        public class StopAsyncTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                [Target] DefaultRequestWorker worker
            )
            {
                var cancellationToken = new CancellationToken(true);
                Func<Task> func = () => worker.StopAsync(cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                [Target] DefaultRequestWorker worker,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => worker.StopAsync(cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldStopTheTimer(
                [Frozen] RequestOptions options,
                [Frozen] ITimer timer,
                [Frozen] ITimerFactory timerFactory,
                [Target] DefaultRequestWorker worker,
                CancellationToken cancellationToken
            )
            {
                await worker.StartAsync(cancellationToken);
                await worker.StopAsync(cancellationToken);

                await timer.Received().Stop();
            }
        }

        [TestFixture]
        public class RunTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                [Target] DefaultRequestWorker worker
            )
            {
                var cancellationToken = new CancellationToken(true);
                Func<Task> func = () => worker.Run(cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                [Target] DefaultRequestWorker worker,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => worker.Run(cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldReceiveMessages(
                [Frozen] IRequestMessageRelay relay,
                [Target] DefaultRequestWorker worker,
                CancellationToken cancellationToken
            )
            {
                await worker.Run(cancellationToken);

                await relay.Received().Receive(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldInvokeAllReceivedMessages(
                RequestMessage message1,
                RequestMessage message2,
                IScope scope1,
                IScope scope2,
                IRequestInvoker invoker1,
                IRequestInvoker invoker2,
                [Frozen] IRequestMessageRelay relay,
                [Frozen] IScopeFactory scopeFactory,
                [Target] DefaultRequestWorker worker,
                CancellationToken cancellationToken
            )
            {
                scope1.GetService<IRequestInvoker>().Returns(invoker1);
                scope2.GetService<IRequestInvoker>().Returns(invoker2);
                scopeFactory.CreateScope().Returns(scope1, scope2);

                relay.Receive(Any<CancellationToken>()).Returns(new[] { message1, message2 });
                await worker.Run(cancellationToken);

                scopeFactory.Received(2).CreateScope();
                scope1.Received().GetService<IRequestInvoker>();
                scope2.Received().GetService<IRequestInvoker>();

                await invoker1.Received().Invoke(Is(message1), Is(cancellationToken));
                await invoker2.Received().Invoke(Is(message2), Is(cancellationToken));
            }
        }
    }
}
