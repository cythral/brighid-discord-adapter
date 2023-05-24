using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Database;
using Brighid.Discord.DependencyInjection;
using Brighid.Discord.Threading;
using Brighid.Discord.Tracing;

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
        private readonly ITracingService tracing;
        private readonly ITransactionFactory transactionFactory;
        private readonly ILogger<DefaultRequestWorker> logger;
        private ITimer? timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestWorker" /> class.
        /// </summary>
        /// <param name="timerFactory">Factory to create timers with.</param>
        /// <param name="transactionFactory">Factory to create transactions with.</param>
        /// <param name="relay">Relay service to send/receive messages through the queue.</param>
        /// <param name="options">Options to use for handling requests.</param>
        /// <param name="scopeFactory">Service to create scopes with.</param>
        /// <param name="tracing">Service for managing app traces.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultRequestWorker(
            ITimerFactory timerFactory,
            ITransactionFactory transactionFactory,
            IRequestMessageRelay relay,
            IOptions<RequestOptions> options,
            IScopeFactory scopeFactory,
            ITracingService tracing,
            ILogger<DefaultRequestWorker> logger
        )
        {
            this.timerFactory = timerFactory;
            this.transactionFactory = transactionFactory;
            this.relay = relay;
            this.options = options.Value;
            this.scopeFactory = scopeFactory;
            this.tracing = tracing;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            timer = timerFactory.CreateTimer(Run, options.PollingInterval, nameof(DefaultRequestWorker));
            await timer.Start(cancellationToken);
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
            logger.LogDebug("Running REST API Queue Worker");

            var messages = await relay.Receive(cancellationToken);
            var tasks = from message in messages select Invoke(message, cancellationToken);
            await Task.WhenAll(tasks);
        }

        private async Task Invoke(RequestMessage message, CancellationToken cancellationToken)
        {
            using var trace = tracing.StartTrace(message.RequestDetails.TraceHeader);
            tracing.AddAnnotation("event", "rest-call");

            using var transaction = transactionFactory.CreateTransaction();
            using var logScope = logger.BeginScope("{@requestId}", message.RequestDetails.Id);
            using var serviceScope = scopeFactory.CreateScope();
            var invoker = serviceScope.GetService<IRequestInvoker>();

            await invoker.Invoke(message, cancellationToken);
            transaction.Complete();
        }
    }
}
