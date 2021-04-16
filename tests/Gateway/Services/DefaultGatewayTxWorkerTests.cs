using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Messages;
using Brighid.Discord.Serialization;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

#pragma warning disable SA1005

namespace Brighid.Discord.Gateway
{
    public class DefaultGatewayTxWorkerTests
    {
        [TestFixture]
        public class StartTests
        {
            [Test, Auto]
            public void StartShouldCreateWorkerThread(
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayUtilsFactory gatewayUtilsFactory,
                [Target] DefaultGatewayTxWorker txWorker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                txWorker.Start(clientWebSocket, source);

                gatewayUtilsFactory.Received().CreateWorkerThread(Is((Func<Task>)txWorker.Run), Is("Gateway TX"));
            }

            [Test, Auto]
            public void StartShouldStartTheWorkerThread(
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IWorkerThread workerThread,
                [Target] DefaultGatewayTxWorker txWorker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                txWorker.Start(clientWebSocket, source);

                workerThread.Received().Start(Is(source));
            }
        }

        [TestFixture]
        public class StopTests
        {
            [Test, Auto]
            public void StopShouldStopTheWorkerThread(
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IWorkerThread workerThread,
                [Target] DefaultGatewayTxWorker txWorker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                txWorker.Start(clientWebSocket, source);
                workerThread.ClearReceivedCalls();
                txWorker.Stop();

                workerThread.Received().Stop();
            }
        }

        [TestFixture]
        public class EmitTests
        {
            [Test, Auto]
            public async Task EmitShouldEmitTheMessageToTheChannel(
                int sequenceNumber,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                await worker.Emit(message, cancellationToken);

                await channel.Received().Write(Is(message), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task EmitShouldThrowIfTheGatewayWasStopped(
                int sequenceNumber,
                [Substitute] IClientWebSocket clientWebSocket,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                worker.Start(clientWebSocket, source);
                source.Cancel();

                var operationCancellationToken = new CancellationToken(false);
                Func<Task> func = () => worker.Emit(message, operationCancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task EmitShouldThrowIfTheOperationWasCanceled(
                int sequenceNumber,
                [Substitute] IClientWebSocket clientWebSocket,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                worker.Start(clientWebSocket, source);

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
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var tries = 0;
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                channel.WaitToRead(Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return false;
                });

                factory.CreateDelay(Any<uint>(), Any<CancellationToken>()).Returns(async x =>
                {
                    if (++tries == 2)
                    {
                        clientWebSocket.State.Returns(WebSocketState.Open);
                    }

                    await Task.CompletedTask;
                });

                clientWebSocket.State.Returns(WebSocketState.Connecting);
                worker.Start(clientWebSocket, source);
                await worker.Run();

                await factory.Received(2).CreateDelay(100, Is(source.Token));
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldNotReadFromChannelIfWaitToReadReturnsFalse(
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                channel.WaitToRead(Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return false;
                });

                clientWebSocket.State.Returns(WebSocketState.Open);
                worker.Start(clientWebSocket, source);
                await worker.Run();

                await channel.DidNotReceive().Read(Is(source.Token));
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldReadFromChannelIfWaitToReadReturnsTrue(
                int sequenceNumber,
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                channel.Read(Any<CancellationToken>()).Returns(new ValueTask<GatewayMessage>(message));
                channel.WaitToRead(Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return true;
                });

                clientWebSocket.State.Returns(WebSocketState.Open);
                worker.Start(clientWebSocket, source);
                await worker.Run();

                await channel.Received().WaitToRead(Is(source.Token));
                await channel.Received().Read(Is(source.Token));
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldSerializeMessageToBytes(
                int sequenceNumber,
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                channel.Read(Any<CancellationToken>()).Returns(new ValueTask<GatewayMessage>(message));
                channel.WaitToRead(Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return true;
                });

                clientWebSocket.State.Returns(WebSocketState.Open);
                worker.Start(clientWebSocket, source);
                await worker.Run();

                await serializer.Received().SerializeToBytes(Is(message), Is(source.Token));
            }

            [Test, Auto, Timeout(1000)]
            public async Task RunShouldWriteBytesToWebSocket(
                int sequenceNumber,
                byte[] bytes,
                [Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IChannel<GatewayMessage> channel,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayTxWorker worker
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                serializer.SerializeToBytes(Any<GatewayMessage>(), Any<CancellationToken>()).Returns(bytes);
                channel.Read(Any<CancellationToken>()).Returns(new ValueTask<GatewayMessage>(message));
                channel.WaitToRead(Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return true;
                });

                clientWebSocket.State.Returns(WebSocketState.Open);
                worker.Start(clientWebSocket, source);
                await worker.Run();

                await clientWebSocket.Received().Send(Is<ArraySegment<byte>>(bytes), Is(WebSocketMessageType.Text), Is(true), Is(source.Token));
            }
        }
    }
}
