using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Events;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Serialization;
using Brighid.Discord.Threading;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Gateway
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

            [Test, Auto]
            public async Task StartShouldSetupGatewayToRestartOnUnexpectedStops(
                [Frozen, Substitute] IWorkerThread workerThread,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                worker.Start(gateway, source);

                workerThread.Received().OnUnexpectedStop = Any<OnUnexpectedStop>();
                var arg = (from call in workerThread.ReceivedCalls()
                           where call.GetMethodInfo().Name.Contains(nameof(workerThread.OnUnexpectedStop))
                           select (OnUnexpectedStop)call.GetArguments()[0]).First();

                await arg();
                await gateway.Received().Restart();
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
            public async Task RunShouldDeserializeTheMessageIfEndOfMessageIsReached(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ISerializer serializer,
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

                await serializer.Received().Deserialize<GatewayMessage>(Is(stream), Is(source.Token));
            }

            [Test, Auto]
            public async Task RunShouldUpdateGatewaySequenceNumberIfEndOfMessageIsReached(
                byte[] bytes,
                int sequenceNumber,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ISerializer serializer,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                serializer.Deserialize<GatewayMessage>(Any<Stream>(), Any<CancellationToken>()).Returns(message);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));
                stream.When(x => x.SetLength(Any<long>())).Do(x =>
                {
                    source.Cancel();
                });

                worker.Start(gateway, source);
                await worker.Run();

                gateway.Received().SequenceNumber = sequenceNumber;
            }

            [Test, Auto]
            public async Task RunShouldRouteTheMessageIfEndOfMessageIsReached(
                byte[] bytes,
                uint interval,
                int sequenceNumber,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IEventRouter router,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var @event = new HelloEvent { HeartbeatInterval = interval };
                var message = new GatewayMessage { SequenceNumber = sequenceNumber, Data = @event };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                serializer.Deserialize<GatewayMessage>(Any<Stream>(), Any<CancellationToken>()).Returns(message);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));
                stream.When(x => x.SetLength(Any<long>())).Do(x =>
                {
                    source.Cancel();
                });

                worker.Start(gateway, source);
                await worker.Run();

                await router.Received().Route(Is(@event), Is(source.Token));
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

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldNotDeserializeTheMessageIfEndOfMessageIsNotReached(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ISerializer serializer,
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

                await serializer.DidNotReceive().Deserialize<GatewayMessage>(Is(stream), Is(source.Token));
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldNotUpdateSequenceNumberIfEndOfMessageIsNotReached(
                byte[] bytes,
                int sequenceNumber,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ISerializer serializer,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                serializer.Deserialize<GatewayMessage>(Any<Stream>(), Any<CancellationToken>()).Returns(message);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, false));
                stream.When(x => x.WriteAsync(Any<ReadOnlyMemory<byte>>(), Any<CancellationToken>())).Do(x =>
                {
                    source.Cancel();
                });

                worker.Start(gateway, source);
                await worker.Run();

                gateway.DidNotReceive().SequenceNumber = sequenceNumber;
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldNotUpdateSequenceNumberIfEndOfMessageIsReachedButSequenceNumberIsNull(
                byte[] bytes,
                int sequenceNumber,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ISerializer serializer,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = null };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                serializer.Deserialize<GatewayMessage>(Any<Stream>(), Any<CancellationToken>()).Returns(message);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));
                stream.When(x => x.SetLength(Any<long>())).Do(x =>
                {
                    source.Cancel();
                });

                gateway.SequenceNumber = sequenceNumber;

                worker.Start(gateway, source);
                await worker.Run();

                gateway.SequenceNumber.Should().Be(sequenceNumber);
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldNotRouteTheMessageIfEndOfMessageIsNotReached(
                byte[] bytes,
                uint interval,
                int sequenceNumber,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IEventRouter router,
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

                await router.DidNotReceive().Route(Any<IGatewayEvent>(), Any<CancellationToken>());
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldNotRouteTheMessageIfEndOfMessageIsReachedButEventDataIsNull(
                byte[] bytes,
                uint interval,
                int sequenceNumber,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IEventRouter router,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));
                serializer.Deserialize<GatewayMessage>(Any<Stream>(), Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return new GatewayMessage { };
                });

                worker.Start(gateway, source);
                await worker.Run();

                await router.DidNotReceive().Route(Any<IGatewayEvent>(), Any<CancellationToken>());
            }
        }
    }
}
