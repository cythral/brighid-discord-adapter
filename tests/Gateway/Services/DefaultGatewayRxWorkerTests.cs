using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Messages;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Gateway
{
    public class DefaultGatewayRxWorkerTests
    {
        [TestFixture]
        public class StartTests
        {
            [Test, Auto]
            public void StartShouldCreateWorkerThread(
                [Frozen, Substitute] IGatewayUtilsFactory gatewayUtilsFactory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker rxWorker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                rxWorker.Start(gateway, source);

                gatewayUtilsFactory.Received().CreateWorkerThread(Is((Func<Task>)rxWorker.Run), Is("Gateway RX"));
            }

            [Test, Auto]
            public void StartShouldStartTheWorkerThread(
                [Frozen, Substitute] IWorkerThread workerThread,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker rxWorker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                rxWorker.Start(gateway, source);

                workerThread.Received().Start(Is(source));
            }
        }

        [TestFixture]
        public class StopTests
        {
            [Test, Auto]
            public void StopShouldStopTheWorkerThread(
                [Frozen, Substitute] IWorkerThread workerThread,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker rxWorker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                rxWorker.Start(gateway, source);
                workerThread.ClearReceivedCalls();
                rxWorker.Stop();

                workerThread.Received().Stop();
            }
        }

        [TestFixture]
        public class EmitTests
        {
            [Test, Auto]
            public async Task EmitShouldEmitTheMessageToTheChannel(
                int sequenceNumber,
                string chunk,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var message = new GatewayMessageChunk { Bytes = Encoding.UTF8.GetBytes(chunk) };
                var cancellationToken = new CancellationToken(false);
                await worker.Emit(message, cancellationToken);

                await channel.Received().Write(Is(message), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task SendShouldThrowIfTheGatewayWasStopped(
                string chunk,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var message = new GatewayMessageChunk { Bytes = Encoding.UTF8.GetBytes(chunk) };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                worker.Start(gateway, source);
                source.Cancel();

                var operationCancellationToken = new CancellationToken(false);
                Func<Task> func = () => worker.Emit(message, operationCancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task SendShouldThrowIfTheOperationWasCanceled(
                string chunk,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var message = new GatewayMessageChunk { Bytes = Encoding.UTF8.GetBytes(chunk) };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                worker.Start(gateway, source);

                var operationCancellationToken = new CancellationToken(true);
                Func<Task> func = () => worker.Emit(message, operationCancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }
        }

        [TestFixture]
        public class RunTests
        {
            [Test, Auto]
            public async Task RunShouldCreateAStream(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, 0, true));
                stream.When(x => x.SetLength(Any<long>())).Do(x =>
                {
                    source.Cancel();
                });

                worker.Start(gateway, source);
                await worker.Run();

                factory.Received().CreateStream();
            }

            [Test, Auto]
            public async Task RunShouldWaitToReadFromTheChannel(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, 0, true));
                stream.When(x => x.SetLength(Any<long>())).Do(x =>
                {
                    source.Cancel();
                });

                worker.Start(gateway, source);
                await worker.Run();

                await channel.Received().WaitToRead(Is(source.Token));
            }

            [Test, Auto]
            public async Task RunShouldNotReadFromTheChannelIfWaitToReadReturnedFalse(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                channel.WaitToRead(Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return false;
                });

                stream.When(x => x.SetLength(Any<long>())).Do(x =>
                {
                    source.Cancel();
                });

                worker.Start(gateway, source);
                await worker.Run();

                await channel.DidNotReceive().Read(Is(source.Token));
            }

            [Test, Auto]
            public async Task RunShouldReadFromTheChannelIfWaitToReadReturnedTrue(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                channel.WaitToRead(Any<CancellationToken>()).Returns(true);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, 0, true));
                stream.When(x => x.SetLength(Any<long>())).Do(x =>
                {
                    source.Cancel();
                });

                worker.Start(gateway, source);
                await worker.Run();

                await channel.Received().Read(Is(source.Token));
            }

            [Test, Auto]
            public async Task RunShouldWriteTheBytesToTheStream(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));
                stream.When(x => x.SetLength(Any<long>())).Do(x =>
                {
                    source.Cancel();
                });

                worker.Start(gateway, source);
                await worker.Run();

                await stream.Received().WriteAsync(Is<ReadOnlyMemory<byte>>(givenBytes => Encoding.UTF8.GetString(bytes) == Encoding.UTF8.GetString(bytes)), Is(source.Token));
            }

            [Test, Auto]
            public async Task RunShouldTruncateTheStreamIfEndOfMessageIsReached(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));
                stream.When(x => x.SetLength(Any<long>())).Do(x =>
                {
                    source.Cancel();
                });

                worker.Start(gateway, source);
                await worker.Run();

                stream.Received().SetLength(0);
            }

            [Test, Auto]
            public async Task RunShouldNotTruncateTheStreamIfEndOfMessageIsNotReached(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, false));
                stream.When(x => x.WriteAsync(Any<ReadOnlyMemory<byte>>(), Any<CancellationToken>())).Do(x =>
                {
                    source.Cancel();
                });

                worker.Start(gateway, source);
                await worker.Run();

                stream.DidNotReceive().SetLength(0);
            }
        }
    }
}
