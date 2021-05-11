using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Metrics;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <inheritdoc />
    [LogCategory("Gateway Restarter")]
    public class DefaultGatewayRestartService : IGatewayRestartService
    {
        private readonly IGatewayUtilsFactory utilsFactory;
        private readonly IMetricReporter reporter;
        private readonly ILogger<DefaultGatewayRestartService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayRestartService" /> class.
        /// </summary>
        /// <param name="utilsFactory">Factory used to create various utilities.</param>
        /// <param name="reporter">Reporter used to report metrics.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultGatewayRestartService(
            IGatewayUtilsFactory utilsFactory,
            IMetricReporter reporter,
            ILogger<DefaultGatewayRestartService> logger
        )
        {
            this.utilsFactory = utilsFactory;
            this.reporter = reporter;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Restart(IGatewayService gateway, bool resume, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Restarting the Gateway Service.");
            var cancellationTokenSource = new CancellationTokenSource();

            _ = reporter.Report(default(GatewayRestartMetric), cancellationToken);
            gateway.SessionId = resume ? gateway.SessionId : null;
            await gateway.Stop();
            await utilsFactory.CreateRandomDelay(1000, 5000, cancellationToken);
            gateway.Start(cancellationTokenSource);
        }
    }
}
