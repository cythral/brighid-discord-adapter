using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Events;
using Brighid.Discord.Messages;

using FluentAssertions;

using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Gateway
{
    public class DefaultGatewayServiceTests
    {
        [TestFixture]
        public class StartTests
        {
            [Test, Auto]
            public void StartShouldCreateWorkerThread(
                [Frozen, Substitute] IGatewayUtilsFactory gatewayUtilsFactory,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                gateway.Start(source);

                gatewayUtilsFactory.Received().CreateWorkerThread(Is((Func<Task>)gateway.Run), Is("Gateway Master"));
            }

            [Test, Auto]
            public void StartShouldCreateWebSocketClient(
                [Frozen, Substitute] IGatewayUtilsFactory gatewayUtilsFactory,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                gateway.Start(source);

                gatewayUtilsFactory.Received().CreateWebSocketClient();
            }

            [Test, Auto]
            public void StartShouldStartTheRxWorker(
                [Frozen, Substitute] IGatewayRxWorker rxWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                gateway.Start(source);

                rxWorker.Received().Start(Is(gateway), Is(source));
            }

            [Test, Auto]
            public void StartShouldStartTheTxWorker(
                [Frozen, Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                gateway.Start(source);

                txWorker.Received().Start(Is(gateway), Is(clientWebSocket), Is(source));
            }

            [Test, Auto]
            public void StartShouldStartTheMasterWorkerThread(
                [Frozen, Substitute] IWorkerThread workerThread,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                gateway.Start(source);

                workerThread.Received().Start(Is(source));
            }

            [Test, Auto]
            public async Task StartShouldSetupGatewayToRestartOnUnexpectedStops(
                [Frozen, Substitute] IGatewayRestartService restartService,
                [Frozen, Substitute] IWorkerThread workerThread,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                gateway.Start(source);

                workerThread.Received().OnUnexpectedStop = Any<OnUnexpectedStop>();
                var arg = (from call in workerThread.ReceivedCalls()
                           where call.GetMethodInfo().Name.Contains(nameof(workerThread.OnUnexpectedStop))
                           select (OnUnexpectedStop)call.GetArguments()[0]).First();

                await arg();
                await restartService.Received().Restart(Is(gateway), Is(true), Any<CancellationToken>());
            }
        }

        [TestFixture]
        public class StopTests
        {
            [Test, Auto]
            public void StopShouldAbortTheWebSocket(
                [Frozen, Substitute] IClientWebSocket clientWebSocket,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                gateway.Start(source);
                gateway.Stop();

                clientWebSocket.Received().Abort();
            }

            [Test, Auto]
            public void StopShouldStopTheWorkerThread(
                [Frozen, Substitute] IWorkerThread workerThread,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                gateway.Start(source);
                gateway.Stop();

                workerThread.Received().Stop();
            }

            [Test, Auto]
            public void StopShouldStopTheRxWorker(
                [Frozen, Substitute] IGatewayRxWorker rxWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                gateway.Start(source);
                gateway.Stop();

                rxWorker.Received().Stop();
            }

            [Test, Auto]
            public void StopShouldStopTheTxWorker(
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                gateway.Start(source);
                gateway.Stop();

                txWorker.Received().Stop();
            }

            [Test, Auto, Timeout(1000)]
            public async Task StopShouldStopTheHeartbeat(
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                factory.CreateDelay(Any<uint>(), Any<CancellationToken>()).Returns(x =>
                {
                    return Task.Delay(10);
                });

                gateway.Start(source);
                gateway.StartHeartbeat(10);
                await Task.Delay(10);
                gateway.Stop();
                await Task.Delay(20);
                txWorker.ClearReceivedCalls();
                await Task.Delay(20);
                await txWorker.DidNotReceiveWithAnyArgs().Emit(Any<GatewayMessage>(), Any<CancellationToken>());
            }

            [Test, Auto, Timeout(1000)]
            public void StopShouldSetIsReadyToFalse(
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                gateway.IsReady = true;
                gateway.Start(source);
                gateway.Stop();

                gateway.IsReady.Should().BeFalse();
            }
        }

        [TestFixture]
        public class RestartTests
        {
            [Test, Auto]
            public async Task ShouldCallTheRestartService(
                bool resume,
                [Frozen, Substitute] IGatewayRestartService restartService,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);

                await gateway.Restart(resume, cancellationToken);

                await restartService.Received().Restart(Is(gateway), Is(resume), Is(cancellationToken));
            }
        }

        [TestFixture]
        public class SendTests
        {
            [Test, Auto]
            public async Task SendShouldEmitTheMessageToTheTxWorker(
                int sequenceNumber,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                gateway.Start(source);
                await gateway.Send(message, cancellationToken);

                await txWorker.Received().Emit(Is(message), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task SendShouldThrowIfTheGatewayWasStopped(
                int sequenceNumber,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                gateway.Start(source);
                source.Cancel();

                var operationCancellationToken = new CancellationToken(false);
                Func<Task> func = () => gateway.Send(message, operationCancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task SendShouldThrowIfTheOperationWasCanceled(
                int sequenceNumber,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var message = new GatewayMessage { SequenceNumber = sequenceNumber };
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                gateway.Start(source);

                var operationCancellationToken = new CancellationToken(true);
                Func<Task> func = () => gateway.Send(message, operationCancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }
        }

        [TestFixture]
        public class StartHeartbeat
        {
            [Test, Auto]
            public void ShouldThrowIfGatewayHasntBeenStarted(
                uint interval,
                [Target] DefaultGatewayService gateway
            )
            {
                Action func = () => gateway.StartHeartbeat(interval);

                func.Should().Throw<OperationCanceledException>();
            }

            [Test, Auto]
            public void ShouldThrowIfGatewayWasStopped(
                uint interval,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                gateway.Start(source);
                source.Cancel();

                Action func = () => gateway.StartHeartbeat(interval);

                func.Should().Throw<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldStartSendingAHeartbeatToTheTxWorker(
                uint interval,
                int sequenceNumber,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                factory.CreateDelay(Any<uint>(), Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return Task.CompletedTask;
                });

                gateway.SequenceNumber = sequenceNumber;
                gateway.Start(source);
                gateway.StartHeartbeat(interval);

                await txWorker.Received().Emit(
                    Is<GatewayMessage>(message =>
                        message.OpCode == GatewayOpCode.Heartbeat &&
                        (HeartbeatEvent?)message.Data == sequenceNumber
                    ),
                    Any<CancellationToken>()
                );
            }
        }

        [TestFixture]
        public class StopHeartbeat
        {
            [Test, Auto, Timeout(1000)]
            public async Task ShouldStopSendingHeartbeatsToTheTxWorker(
                uint interval,
                int sequenceNumber,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                factory.CreateDelay(Any<uint>(), Any<CancellationToken>()).Returns(x =>
                {
                    return Task.Delay(10);
                });

                gateway.SequenceNumber = sequenceNumber;
                gateway.Start(source);
                gateway.StartHeartbeat(interval);
                await Task.Delay(10);
                gateway.StopHeartbeat();
                await Task.Delay(20);
                txWorker.ClearReceivedCalls();
                await Task.Delay(20);
                await txWorker.DidNotReceiveWithAnyArgs().Emit(Any<GatewayMessage>(), Any<CancellationToken>());
            }
        }

        [TestFixture]
        public class RunTests
        {
            [Test, Auto]
            public async Task RunShouldThrowIfTheGatewayWasStopped(
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                gateway.Start(source);
                source.Cancel();

                Func<Task> func = () => gateway.Run();
                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task RunShouldConnectToTheWebSocketServer(
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IClientWebSocket webSocket,
                [Frozen, Substitute] IGatewayRxWorker rxWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                webSocket.Receive(Any<Memory<byte>>(), Any<CancellationToken>()).Returns(x => new ValueWebSocketReceiveResult(0, WebSocketMessageType.Text, true));
                rxWorker.Emit(Any<GatewayMessageChunk>(), Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return Task.CompletedTask;
                });

                gateway.Start(source);
                await gateway.Run();

                await webSocket.Received().Connect(Is(options.Value.Uri), Is(source.Token));
            }
#pragma warning disable SA1005

            [Test, Auto]
            public async Task RunShouldReceiveChunksFromTheWebSocket(
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IClientWebSocket webSocket,
                [Frozen, Substitute] IGatewayRxWorker rxWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                webSocket.Receive(Any<Memory<byte>>(), Any<CancellationToken>()).Returns(x => new ValueWebSocketReceiveResult(0, WebSocketMessageType.Text, true));
                rxWorker.Emit(Any<GatewayMessageChunk>(), Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return Task.CompletedTask;
                });

                gateway.Start(source);
                await gateway.Run();

                await webSocket.Received().Receive(Any<Memory<byte>>(), Is(source.Token));
            }

            [Test, Auto]
            public async Task RunShouldEmitMessageChunksToTheRxWorker(
                string messageChunk,
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IClientWebSocket webSocket,
                [Frozen, Substitute] IGatewayRxWorker rxWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var buffer = new byte[messageChunk.Length];
                var memoryBuffer = new Memory<byte>(buffer);
                var cancellationToken = new CancellationToken(false);
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                gateway.SetPrivateField("buffer", buffer);
                gateway.SetPrivateField("memoryBuffer", memoryBuffer);

                webSocket.Receive(Any<Memory<byte>>(), Any<CancellationToken>()).Returns(x =>
                {
                    var buffer = x.ArgAt<Memory<byte>>(0);
                    ((Memory<byte>)Encoding.UTF8.GetBytes(messageChunk)).CopyTo(buffer);
                    return new ValueWebSocketReceiveResult(messageChunk.Length, WebSocketMessageType.Text, true);
                });

                rxWorker.Emit(Any<GatewayMessageChunk>(), Any<CancellationToken>()).Returns(x =>
                {
                    source.Cancel();
                    return Task.CompletedTask;
                });

                gateway.Start(source);
                await gateway.Run();

                await rxWorker.Received().Emit(
                    Is<GatewayMessageChunk>(chunk =>
                        Encoding.UTF8.GetString(chunk.Bytes.ToArray()) == messageChunk &&
                        chunk.Count == messageChunk.Length &&
                        chunk.EndOfMessage
                    ),
                    Is(source.Token)
                );
            }
        }
    }
}
