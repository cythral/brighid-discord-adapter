using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using NUnit.Framework;

namespace Brighid.Discord.Gateway
{
    public class DefaultGatewayUtilsFactoryTests
    {
        [Test, Auto]
        public void CreateShouldCreateWorkerWithLogger(
            string workerName,
            [Substitute] Func<Task> runAsync,
            [Frozen, Substitute] ILogger<DefaultGatewayUtilsFactory> logger,
            [Target] DefaultGatewayUtilsFactory factory
        )
        {
            var workerThread = factory.CreateWorkerThread(runAsync, workerName);
            var cancellationToken = new CancellationToken(false);
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            logger.ClearReceivedCalls();

            workerThread.Start(cancellationTokenSource);
            workerThread.Stop();

            logger.ReceivedWithAnyArgs().Log(LogLevel.Information, null);
        }

        [Test, Auto]
        public void CreateShouldCreateWorkerWithName(
            string workerName,
            [Substitute] Func<Task> runAsync,
            [Frozen, Substitute] ILogger<DefaultGatewayUtilsFactory> logger,
            [Target] DefaultGatewayUtilsFactory factory
        )
        {
            var workerThread = factory.CreateWorkerThread(runAsync, workerName);

            workerThread.Name.Should().Be(workerName);
        }

        [Test, Auto]
        public async Task CreateShouldCreateWorkerWithRunAsync(
            string workerName,
            [Substitute] Func<Task> runAsync,
            [Frozen, Substitute] ILogger<DefaultGatewayUtilsFactory> logger,
            [Target] DefaultGatewayUtilsFactory factory
        )
        {
            var workerThread = factory.CreateWorkerThread(runAsync, workerName);
            var cancellationToken = new CancellationToken(false);
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            workerThread.Start(cancellationTokenSource);
            workerThread.Stop();

            await runAsync.Received()();
        }
    }
}
