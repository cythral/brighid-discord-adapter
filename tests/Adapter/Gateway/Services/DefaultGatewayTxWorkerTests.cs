using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Serialization;
using Brighid.Discord.Threading;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

#pragma warning disable SA1005

namespace Brighid.Discord.Adapter.Gateway
{
    public class DefaultGatewayTxWorkerTests
    {
        [TestFixture]
        public class StartTests
        {
            [Test, Auto]
            public async Task StartShouldCreateTimer(
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Target] DefaultGatewayTxWorker txWorker
            )
            {
                await txWorker.Start(gateway, clientWebSocket);

                timerFactory.Received().CreateTimer(Is((AsyncTimerCallback)txWorker.Run), Is(0), Is("Gateway TX"));
            }

            [Test, Auto]
            public async Task StartShouldStartTheWorkerThread(
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ITimer timer,
                [Target] DefaultGatewayTxWorker txWorker
            )
            {
                await txWorker.Start(gateway, clientWebSocket);

                await timer.Received().Start();
            }

            [Test, Auto]
            public async Task StartShouldSetupGatewayToRestartOnUnexpectedStops(
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] ITimer timer,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                await worker.Start(gateway, clientWebSocket);

                timer.Received().StopOnException = Is(true);
                timer.Received().OnUnexpectedStop = Any<OnUnexpectedTimerStop>();
                var arg = (from call in timer.ReceivedCalls()
                           where call.GetMethodInfo().Name.Contains(nameof(timer.OnUnexpectedStop))
                           select (OnUnexpectedTimerStop)call.GetArguments()[0]).First();

                await arg();
                await gateway.Received().Restart();
            }
        }

        [TestFixture]
        public class StopTests
        {
            [Test, Auto]
            public async Task StopShouldStopTheWorkerThread(
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] ITimer timer,
                [Target] DefaultGatewayTxWorker txWorker
            )
            {
                await txWorker.Start(gateway, clientWebSocket);
                timer.ClearReceivedCalls();
                await txWorker.Stop();

                await timer.Received().Stop();
            }
        }

        [TestFixture]
        public class EmitTests
        {
            [Test, Auto]
            public async Task EmitShouldEmitTheMessageToTheChannel(
                int sequenceNumber,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] IClientWebSocket webSocket,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);

                await worker.Start(gateway, webSocket);
                await worker.Emit(message, cancellationToken);

                await channel.Received().Write(Is(message), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task EmitShouldThrowIfTheGatewayWasStopped(
                int sequenceNumber,
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                await worker.Start(gateway, clientWebSocket);
                await worker.Stop();

                var operationCancellationToken = new CancellationToken(false);
                Func<Task> func = () => worker.Emit(message, operationCancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task EmitShouldThrowIfTheOperationWasCanceled(
                int sequenceNumber,
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayService gateway,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);

                await worker.Start(gateway, clientWebSocket);

                var operationCancellationToken = new CancellationToken(true);
                Func<Task> func = () => worker.Emit(message, operationCancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }
        }

        [TestFixture]
        public class RunTests
        {
            [Test, Auto, Timeout(1000)]
            public async Task RunShouldWaitUntilTheWebSocketIsOpen(
                int sequenceNumber,
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var tries = 0;
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                clientWebSocket.State.Returns(WebSocketState.Connecting);

                channel.Read(Any<CancellationToken>()).Returns(new ValueTask<GatewayMessage>(message));
                channel.WaitToRead(Any<CancellationToken>()).Returns(true);
                factory.CreateDelay(Any<uint>(), Any<CancellationToken>()).Returns(async x =>
                {
                    if (++tries == 2)
                    {
                        clientWebSocket.State.Returns(WebSocketState.Open);
                    }

                    await Task.CompletedTask;
                });

                await worker.Start(gateway, clientWebSocket);
                await worker.Run(cancellationToken);
                await factory.Received(2).CreateDelay(100, Is(cancellationToken));
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldNotReadFromChannelIfWaitToReadReturnsFalse(
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);

                channel.WaitToRead(Any<CancellationToken>()).Returns(false);

                clientWebSocket.State.Returns(WebSocketState.Open);
                await worker.Start(gateway, clientWebSocket);
                await worker.Run(cancellationToken);

                await channel.DidNotReceive().Read(Is(cancellationToken));
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldReadFromChannelIfWaitToReadReturnsTrue(
                int sequenceNumber,
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);

                channel.Read(Any<CancellationToken>()).Returns(new ValueTask<GatewayMessage>(message));

                clientWebSocket.State.Returns(WebSocketState.Open);
                await worker.Start(gateway, clientWebSocket);
                await worker.Run(cancellationToken);

                await channel.Received().WaitToRead(Is(cancellationToken));
                await channel.Received().Read(Is(cancellationToken));
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldSerializeMessageToBytes(
                int sequenceNumber,
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);

                channel.Read(Any<CancellationToken>()).Returns(new ValueTask<GatewayMessage>(message));

                clientWebSocket.State.Returns(WebSocketState.Open);
                await worker.Start(gateway, clientWebSocket);
                await worker.Run(cancellationToken);

                serializer.Received().SerializeToBytes(Is(message));
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldWriteBytesToWebSocket(
                int sequenceNumber,
                byte[] bytes,
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayService gateway,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);

                serializer.SerializeToBytes(Any<GatewayMessage>()).Returns(bytes);
                channel.Read(Any<CancellationToken>()).Returns(new ValueTask<GatewayMessage>(message));

                clientWebSocket.State.Returns(WebSocketState.Open);
                await worker.Start(gateway, clientWebSocket);
                await worker.Run(cancellationToken);

                await clientWebSocket.Received().Send(Is<ArraySegment<byte>>(bytes), Is(WebSocketMessageType.Text), Is(true), Is(cancellationToken));
            }
        }
    }
}
