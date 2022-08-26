using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter.Gateway
{
    /// <inheritdoc />
    [LogCategory("Gateway Restarter")]
    public class DefaultGatewayRestartService : IGatewayRestartService
    {
        private readonly IGatewayUtilsFactory utilsFactory;
        private readonly ILogger<DefaultGatewayRestartService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGatewayRestartService" /> class.
        /// </summary>
        /// <param name="utilsFactory">Factory used to create various utilities.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DefaultGatewayRestartService(
            IGatewayUtilsFactory utilsFactory,
            ILogger<DefaultGatewayRestartService> logger
        )
        {
            this.utilsFactory = utilsFactory;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task Restart(IGatewayService gateway, bool resume, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation(LogEvents.GatewayRestarted, "Restarting the Gateway Service.");

            gateway.SessionId = resume ? gateway.SessionId : null;
            await gateway.StopAsync(cancellationToken);
            await utilsFactory.CreateRandomDelay(1000, 5000, cancellationToken);
            await gateway.StartAsync(cancellationToken);
        }
    }
}
