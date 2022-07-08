using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Adapter.Events;
using Brighid.Discord.Adapter.Messages;
using Brighid.Discord.Threading;

using FluentAssertions;

using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Gateway
{
    public class DefaultGatewayServiceTests
    {
        [TestFixture]
        [Category("Unit")]
        public class SetReadyStateTests
        {
            [Test, Auto]
            public void ShouldAddReadyToState(
                [Target] DefaultGatewayService service
            )
            {
                service.SetReadyState(true);

                service.State.HasFlag(GatewayState.Ready).Should().BeTrue();
            }

            [Test, Auto]
            public void ShouldRemoveReadyFromState(
                [Target] DefaultGatewayService service
            )
            {
                service.SetPrivateProperty(nameof(service.State), GatewayState.Ready);
                service.SetReadyState(false);

                service.State.HasFlag(GatewayState.Ready).Should().BeFalse();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class ThrowIfNotReadyTests
        {
            [Test, Auto]
            public void ShouldThrowCanceledIfNotReady(
                [Target] DefaultGatewayService service
            )
            {
                service.SetPrivateProperty("State", GatewayState.Running);
                Action func = service.ThrowIfNotReady;
                func.Should().Throw<OperationCanceledException>();
            }

            [Test, Auto]
            public void ShouldNotThrowCanceledIfReady(
                [Target] DefaultGatewayService service
            )
            {
                service.SetPrivateProperty("State", GatewayState.Running | GatewayState.Ready);
                Action func = service.ThrowIfNotReady;
                func.Should().NotThrow<OperationCanceledException>();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class StartTests
        {
            [Test, Auto]
            public async Task StartShouldCreateWorker(
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Target] DefaultGatewayService gateway
            )
            {
                await gateway.StartAsync();

                timerFactory.Received().CreateTimer(Is((AsyncTimerCallback)gateway.Run), Is(0), Is("Gateway Master"));
            }

            [Test, Auto]
            public async Task StartShouldCreateWebSocketClient(
                [Frozen, Substitute] IGatewayUtilsFactory gatewayUtilsFactory,
                [Target] DefaultGatewayService gateway
            )
            {
                await gateway.StartAsync();

                gatewayUtilsFactory.Received().CreateWebSocketClient();
            }

            [Test, Auto]
            public async Task StartShouldStartTheRxWorker(
                [Frozen, Substitute] IGatewayRxWorker rxWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                await gateway.StartAsync();

                await rxWorker.Received().Start(Is(gateway));
            }

            [Test, Auto]
            public async Task StartShouldStartTheTxWorker(
                [Frozen, Substitute] IClientWebSocket clientWebSocket,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                await gateway.StartAsync();

                await txWorker.Received().Start(Is(gateway), Is(clientWebSocket));
            }

            [Test, Auto]
            public async Task StartShouldStartTheMasterWorker(
                [Frozen, Substitute] ITimer timer,
                [Target] DefaultGatewayService gateway
            )
            {
                await gateway.StartAsync();

                await timer.Received().Start();
            }

            [Test, Auto]
            public async Task StartShouldSetupGatewayToRestartOnUnexpectedStops(
                [Frozen, Substitute] IGatewayRestartService restartService,
                [Frozen, Substitute] ITimer worker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                await gateway.StartAsync();

                worker.Received().StopOnException = Is(true);
                worker.Received().OnUnexpectedStop = Any<OnUnexpectedTimerStop>();
                var arg = (from call in worker.ReceivedCalls()
                           where call.GetMethodInfo().Name.Contains(nameof(worker.OnUnexpectedStop))
                           select (OnUnexpectedTimerStop)call.GetArguments()[0]).First();

                await arg();
                await restartService.Received().Restart(Is(gateway), Is(true), Any<CancellationToken>());
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class StopTests
        {
            [Test, Auto]
            public async Task StopShouldAbortTheWebSocket(
                [Frozen, Substitute] IClientWebSocket clientWebSocket,
                [Target] DefaultGatewayService gateway
            )
            {
                await gateway.StartAsync();
                await gateway.StopAsync();

                clientWebSocket.Received().Abort();
            }

            [Test, Auto]
            public async Task StopShouldStopTheWorker(
                [Frozen, Substitute] ITimer worker,
                [Target] DefaultGatewayService gateway
            )
            {
                await gateway.StartAsync();
                await gateway.StopAsync();

                await worker.Received().Stop();
            }

            [Test, Auto]
            public async Task StopShouldStopTheRxWorker(
                [Frozen, Substitute] IGatewayRxWorker rxWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                await gateway.StartAsync();
                await gateway.StopAsync();

                await rxWorker.Received().Stop();
            }

            [Test, Auto]
            public async Task StopShouldStopTheTxWorker(
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                await gateway.StartAsync();
                await gateway.StopAsync();

                await txWorker.Received().Stop();
            }

            [Test, Auto, Timeout(1000)]
            public async Task StopShouldStopTheHeartbeat(
                ITimer heartbeat,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);

                timerFactory.CreateTimer(Any<AsyncTimerCallback>(), Any<int>(), Is("Heartbeat")).Returns(heartbeat);

                await gateway.StartAsync();
                await gateway.StartHeartbeat(10);
                await gateway.StopAsync();

                await heartbeat.Received().Stop();
            }

            [Test, Auto, Timeout(1000)]
            public async Task StopShouldSetRemoveReadyFromState(
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Target] DefaultGatewayService gateway
            )
            {
                await gateway.StartAsync();
                await gateway.StopAsync();

                gateway.State.HasFlag(GatewayState.Ready).Should().BeFalse();
            }
        }

        [TestFixture]
        [Category("Unit")]
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
        [Category("Unit")]
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

                await gateway.StartAsync();
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

                await gateway.StartAsync();
                await gateway.StopAsync();

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

                await gateway.StartAsync();

                var operationCancellationToken = new CancellationToken(true);
                Func<Task> func = () => gateway.Send(message, operationCancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class StartHeartbeat
        {
            [Test, Auto]
            public async Task ShouldThrowIfGatewayHasntBeenStarted(
                uint interval,
                [Target] DefaultGatewayService gateway
            )
            {
                Func<Task> func = () => gateway.StartHeartbeat(interval);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldThrowIfGatewayWasStopped(
                uint interval,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);

                await gateway.StartAsync();
                await gateway.StopAsync();

                Func<Task> func = () => gateway.StartHeartbeat(interval);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldStartSendingAHeartbeatToTheTxWorker(
                uint interval,
                int sequenceNumber,
                [Frozen] ITimer timer,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);

                gateway.SequenceNumber = sequenceNumber;
                await gateway.StartAsync();
                await gateway.StartHeartbeat(interval);

                timerFactory.Received().CreateTimer(Is<AsyncTimerCallback>(gateway.Heartbeat), Is((int)interval), Is("Heartbeat"));
                await timer.Received().Start();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class StopHeartbeat
        {
            [Test, Auto, Timeout(1000)]
            public async Task ShouldStopSendingHeartbeatsToTheTxWorker(
                uint interval,
                int sequenceNumber,
                [Frozen] ITimer heartbeat,
                [Frozen, Substitute] IGatewayUtilsFactory factory,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Frozen, Substitute] ITimerFactory timerFactory,
                [Target] DefaultGatewayService gateway
            )
            {
                gateway.SequenceNumber = sequenceNumber;
                await gateway.StartAsync();
                await gateway.StartHeartbeat(interval);
                await gateway.StopHeartbeat();

                await heartbeat.Received().Stop();
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class RunTests
        {
            [Test, Auto]
            public async Task RunShouldThrowIfTheGatewayWasStopped(
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);

                await gateway.StartAsync();
                await gateway.StopAsync();

                Func<Task> func = () => gateway.Run();
                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task RunShouldConnectToTheWebSocketServerIfNotConnected(
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IClientWebSocket webSocket,
                [Frozen, Substitute] IGatewayRxWorker rxWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                webSocket.State.Returns(WebSocketState.None);
                webSocket.Receive(Any<Memory<byte>>(), Any<CancellationToken>()).Returns(x => new ValueWebSocketReceiveResult(0, WebSocketMessageType.Text, true));

                await gateway.StartAsync();
                await gateway.Run(cancellationToken);

                await webSocket.Received().Connect(Is(options.Value.Uri), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task RunShouldNotConnectToTheWebSocketServerIfAlreadyConnected(
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IClientWebSocket webSocket,
                [Frozen, Substitute] IGatewayRxWorker rxWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(false);
                webSocket.State.Returns(WebSocketState.Open);
                webSocket.Receive(Any<Memory<byte>>(), Any<CancellationToken>()).Returns(x => new ValueWebSocketReceiveResult(0, WebSocketMessageType.Text, true));

                await gateway.StartAsync();
                await gateway.Run(cancellationToken);

                await webSocket.DidNotReceive().Connect(Is(options.Value.Uri), Is(cancellationToken));
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
                webSocket.Receive(Any<Memory<byte>>(), Any<CancellationToken>()).Returns(x => new ValueWebSocketReceiveResult(0, WebSocketMessageType.Text, true));

                await gateway.StartAsync();
                await gateway.Run(cancellationToken);

                await webSocket.Received().Receive(Any<Memory<byte>>(), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task RunShouldEmitMessageChunksToTheRxWorker(
                string messageChunk,
                [Frozen] CancellationTokenSource cancellationTokenSource,
                [Frozen, Options] IOptions<GatewayOptions> options,
                [Frozen, Substitute] IClientWebSocket webSocket,
                [Frozen, Substitute] IGatewayRxWorker rxWorker,
                [Target] DefaultGatewayService gateway
            )
            {
                var buffer = new byte[messageChunk.Length];
                var memoryBuffer = new Memory<byte>(buffer);
                var cancellationToken = new CancellationToken(false);

                gateway.SetPrivateField("buffer", buffer);
                gateway.SetPrivateField("memoryBuffer", memoryBuffer);

                webSocket.Receive(Any<Memory<byte>>(), Any<CancellationToken>()).Returns(x =>
                {
                    var buffer = x.ArgAt<Memory<byte>>(0);
                    ((Memory<byte>)Encoding.UTF8.GetBytes(messageChunk)).CopyTo(buffer);
                    return new ValueWebSocketReceiveResult(messageChunk.Length, WebSocketMessageType.Text, true);
                });

                await gateway.StartAsync();
                await gateway.Run(cancellationToken);

                await rxWorker.Received().Emit(
                    Is<GatewayMessageChunk>(chunk =>
                        Encoding.UTF8.GetString(chunk.Bytes.ToArray()) == messageChunk &&
                        chunk.Count == messageChunk.Length &&
                        chunk.EndOfMessage
                    ),
                    Is(cancellationToken)
                );
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class HeartbeatTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCancelled(
                [Target] DefaultGatewayService gateway
            )
            {
                var cancellationToken = new CancellationToken(true);
                Func<Task> func = () => gateway.Heartbeat(cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCancelled(
                [Target] DefaultGatewayService gateway,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => gateway.Heartbeat(cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldSendAHeartbeatToTheTxWorker(
                int sequenceNumber,
                [Frozen, Substitute] IGatewayTxWorker txWorker,
                [Target] DefaultGatewayService gateway,
                CancellationToken cancellationToken
            )
            {
                gateway.SequenceNumber = sequenceNumber;
                await gateway.Heartbeat(cancellationToken);

                await txWorker.Received().Emit(
                    Is<GatewayMessage>(message =>
                        message.OpCode == GatewayOpCode.Heartbeat &&
                        (HeartbeatEvent?)message.Data == sequenceNumber
                    ),
                    Any<CancellationToken>()
                );
            }
        }
    }
}
