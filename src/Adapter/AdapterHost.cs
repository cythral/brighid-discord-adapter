using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Gateway;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brighid.Discord.Adapter
{
    /// <summary>
    /// The application runner.
    /// </summary>
    [LogCategory(LogCategoryName)]
    public class AdapterHost : IHost
    {
        private const string LogCategoryName = "Adapter Host";
        private readonly IEnumerable<IHostedService> hostedServices;
        private readonly ILogger<AdapterHost> logger;
        private IGatewayService? gateway;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterHost" /> class.
        /// </summary>
        /// <param name="hostedServices">List of hosted services to start.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        /// <param name="services">The service provider to use for accessing host services.</param>
        public AdapterHost(
            IEnumerable<IHostedService> hostedServices,
            ILogger<AdapterHost> logger,
            IServiceProvider services
        )
        {
            Services = services;
            this.hostedServices = hostedServices;
            this.logger = logger;
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Starting.");

            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var tasks = from service in hostedServices select service.StartAsync(cancellationToken);
            await Task.WhenAll(tasks);

            gateway = Services.GetRequiredService<IGatewayService>();
            gateway.Start(source);

            logger.LogInformation("Started.");
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (gateway == null)
            {
                logger.LogInformation("Nothing to stop.");
                return;
            }

            logger.LogInformation("Stopping.");

            var tasks = from service in hostedServices select service.StopAsync(cancellationToken);
            await Task.WhenAll(tasks);

            await gateway.Stop();
            logger.LogInformation("Stopped.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
