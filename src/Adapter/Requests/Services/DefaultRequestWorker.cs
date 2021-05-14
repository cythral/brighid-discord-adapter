using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.DependencyInjection;
using Brighid.Discord.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#pragma warning disable IDE0044, CS0414, CS0649

namespace Brighid.Discord.Adapter.Requests
{
    /// <inheritdoc />
    public class DefaultRequestWorker : IRequestWorker
    {
        private readonly RequestOptions options;
        private readonly IRequestMessageRelay relay;
        private readonly ITimerFactory timerFactory;
        private readonly IScopeFactory scopeFactory;
        private readonly ILogger<DefaultRequestWorker> logger;
        private ITimer? timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestWorker" /> class.
        /// </summary>
        /// <param name="timerFactory">Factory to create timers with.</param>
        /// <param name="relay">Relay service to send/receive messages through the queue.</param>
        /// <param name="options">Options to use for handling requests.</param>
        /// <param name="scopeFactory">Service to create scopes with.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultRequestWorker(
            ITimerFactory timerFactory,
            IRequestMessageRelay relay,
            IOptions<RequestOptions> options,
            IScopeFactory scopeFactory,
            ILogger<DefaultRequestWorker> logger
        )
        {
            this.timerFactory = timerFactory;
            this.relay = relay;
            this.options = options.Value;
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            timer = timerFactory.CreateTimer(Run, options.PollingInterval, nameof(DefaultRequestWorker));
            await timer.Start();
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            timer?.Stop();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs the request worker.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task Run(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Running REST API Queue Worker");
            var messages = await relay.Receive(cancellationToken);

            foreach (var message in messages)
            {
                _ = Invoke(message, cancellationToken);
            }
        }

        private async Task Invoke(RequestMessage message, CancellationToken cancellationToken)
        {
            using var logScope = logger.BeginScope("{@requestId}", message.RequestDetails.Id);
            using var serviceScope = scopeFactory.CreateScope();
            var invoker = serviceScope.GetService<IRequestInvoker>();
            await invoker.Invoke(message, cancellationToken);
        }
    }
}
