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
        [Category("Unit")]
        public class StartTests
        {
            [Test, Auto]
            public async Task StartShouldCreateWorkerThread(
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker rxWorker
            )
            {
                var cancellationToken = new CancellationToken(false);
                await rxWorker.Start(gateway, cancellationToken);

                timerFactory.Received().CreateTimer(Is((AsyncTimerCallback)rxWorker.Run), Is(0), Is("Gateway RX"));
            }

            [Test, Auto]
            public async Task StartShouldStartTheTimer(
                [Frozen, Substitute] ITimer timer,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker rxWorker,
                CancellationToken cancellationToken
            )
            {
                await rxWorker.Start(gateway, cancellationToken);

                await timer.Received().Start(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task StartShouldSetupGatewayToRestartOnUnexpectedStops(
                [Frozen, Substitute] ITimer timer,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                await worker.Start(gateway, cancellationToken);

                timer.Received().StopOnException = Is(true);
                timer.Received().OnUnexpectedStop = Any<OnUnexpectedTimerStop>();
                var arg = (from call in timer.ReceivedCalls()
                           where call.GetMethodInfo().Name.Contains(nameof(timer.OnUnexpectedStop))
                           select (OnUnexpectedTimerStop)call.GetArguments()[0]!).First();

                await arg();
                await gateway.Received().Restart();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class StopTests
        {
            [Test, Auto]
            public async Task StopShouldStopTheWorkerThread(
                [Frozen, Substitute] ITimer timer,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker rxWorker,
                CancellationToken cancellationToken
            )
            {
                await rxWorker.Start(gateway, cancellationToken);
                await rxWorker.Stop();

                await timer.Received().Stop();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class EmitTests
        {
            [Test, Auto]
            public async Task EmitShouldEmitTheMessageToTheChannel(
                int sequenceNumber,
                string chunk,
                IGatewayService gateway,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Target] DefaultGatewayRxWorker worker
            )
            {
                var message = new GatewayMessageChunk { Bytes = Encoding.UTF8.GetBytes(chunk) };
                var cancellationToken = new CancellationToken(false);

                await worker.Start(gateway, cancellationToken);
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

                await worker.Start(gateway, cancellationToken);
                await worker.Stop();

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

                await worker.Start(gateway, cancellationToken);

                var operationCancellationToken = new CancellationToken(true);
                Func<Task> func = () => worker.Emit(message, operationCancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class RunTests
        {
            [Test, Auto]
            public async Task RunShouldCreateAStream(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker,
                CancellationToken cancellationToken
            )
            {
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, 0, true));

                await worker.Start(gateway, cancellationToken);
                await worker.Run();
                await Task.WhenAll(worker.TaskQueue.Values);

                factory.Received().CreateStream();
            }

            [Test, Auto]
            public async Task RunShouldWaitToReadFromTheChannel(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker,
                CancellationToken cancellationToken
            )
            {
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, 0, true));

                await worker.Start(gateway, cancellationToken);
                await worker.Run(cancellationToken);
                await Task.WhenAll(worker.TaskQueue.Values);

                await channel.Received().WaitToRead(Is(cancellationToken));
            }

            [Test, Auto]
            public async Task RunShouldNotReadFromTheChannelIfWaitToReadReturnedFalse(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker,
                CancellationToken cancellationToken
            )
            {
                channel.WaitToRead(Any<CancellationToken>()).Returns(false);

                await worker.Start(gateway, cancellationToken);
                await worker.Run(cancellationToken);
                await Task.WhenAll(worker.TaskQueue.Values);

                await channel.DidNotReceive().Read(Is(cancellationToken));
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
                channel.WaitToRead(Any<CancellationToken>()).Returns(true);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, 0, true));

                await worker.Start(gateway, cancellationToken);
                await worker.Run(cancellationToken);
                await Task.WhenAll(worker.TaskQueue.Values);

                await channel.Received().Read(Is(cancellationToken));
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
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));

                await worker.Start(gateway, cancellationToken);
                await worker.Run(cancellationToken);
                await Task.WhenAll(worker.TaskQueue.Values);

                await stream.Received().WriteAsync(Is<ReadOnlyMemory<byte>>(givenBytes => Encoding.UTF8.GetString(bytes) == Encoding.UTF8.GetString(bytes)), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task RunShouldTruncateTheStreamIfEndOfMessageIsReached(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker,
                CancellationToken cancellationToken
            )
            {
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));

                await worker.Start(gateway, cancellationToken);
                await worker.Run();
                await Task.WhenAll(worker.TaskQueue.Values);

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
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));

                await worker.Start(gateway, cancellationToken);
                await worker.Run(cancellationToken);
                await Task.WhenAll(worker.TaskQueue.Values);

                await serializer.Received().Deserialize<GatewayMessage>(Any<Stream>(), Is(cancellationToken));
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
                [Target] DefaultGatewayRxWorker worker,
                CancellationToken cancellationToken
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };

                serializer.Deserialize<GatewayMessage>(Any<Stream>(), Any<CancellationToken>()).Returns(message);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));

                await worker.Start(gateway, cancellationToken);
                await worker.Run();
                await Task.WhenAll(worker.TaskQueue.Values);

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
                serializer.Deserialize<GatewayMessage>(Any<Stream>(), Any<CancellationToken>()).Returns(message);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));

                await worker.Start(gateway, cancellationToken);
                await worker.Run(cancellationToken);
                await Task.WhenAll(worker.TaskQueue.Values);

                await router.Received().Route(Is(@event), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task RunShouldNotTruncateTheStreamIfEndOfMessageIsNotReached(
                byte[] bytes,
                [Frozen, Substitute] Stream stream,
                [Frozen, Substitute] IChannel<GatewayMessageChunk> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayRxWorker worker,
                CancellationToken cancellationToken
            )
            {
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, false));

                await worker.Start(gateway, cancellationToken);
                await worker.Run();
                await Task.WhenAll(worker.TaskQueue.Values);

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
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, false));

                await worker.Start(gateway, cancellationToken);
                await worker.Run(cancellationToken);
                await Task.WhenAll(worker.TaskQueue.Values);

                await serializer.DidNotReceive().Deserialize<GatewayMessage>(Is(stream), Is(cancellationToken));
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldNotDeserializeTheMessageIfMessageIsZeroBytes(
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
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(Array.Empty<byte>(), 0, true));

                await worker.Start(gateway, cancellationToken);
                await worker.Run(cancellationToken);
                await Task.WhenAll(worker.TaskQueue.Values);

                await serializer.DidNotReceive().Deserialize<GatewayMessage>(Is(stream), Is(cancellationToken));
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
                [Target] DefaultGatewayRxWorker worker,
                CancellationToken cancellationToken
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };

                serializer.Deserialize<GatewayMessage>(Any<Stream>(), Any<CancellationToken>()).Returns(message);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, false));

                await worker.Start(gateway, cancellationToken);
                await worker.Run();
                await Task.WhenAll(worker.TaskQueue.Values);

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
                [Target] DefaultGatewayRxWorker worker,
                CancellationToken cancellationToken
            )
            {
                var message = new GatewayMessage { SequenceNumber = null };

                serializer.Deserialize<GatewayMessage>(Any<Stream>(), Any<CancellationToken>()).Returns(message);
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));

                gateway.SequenceNumber = sequenceNumber;

                await worker.Start(gateway, cancellationToken);
                await worker.Run();
                await Task.WhenAll(worker.TaskQueue.Values);

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
                [Target] DefaultGatewayRxWorker worker,
                CancellationToken cancellationToken
            )
            {
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, false));

                await worker.Start(gateway, cancellationToken);
                await worker.Run();
                await Task.WhenAll(worker.TaskQueue.Values);

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
                [Target] DefaultGatewayRxWorker worker,
                CancellationToken cancellationToken
            )
            {
                channel.Read(Any<CancellationToken>()).Returns(new GatewayMessageChunk(bytes, bytes.Length, true));

                await worker.Start(gateway, cancellationToken);
                await worker.Run();
                await Task.WhenAll(worker.TaskQueue.Values);

                await router.DidNotReceive().Route(Any<IGatewayEvent>(), Any<CancellationToken>());
            }
        }
    }
}
