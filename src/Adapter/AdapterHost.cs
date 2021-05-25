using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Database;
using Brighid.Discord.RestClient.Client;

using Microsoft.EntityFrameworkCore;
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
        private readonly IHostEnvironment environment;
        private readonly ILogger<AdapterHost> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterHost" /> class.
        /// </summary>
        /// <param name="hostedServices">List of hosted services to start.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        /// <param name="environment">Service used to get info about the environment.</param>
        /// <param name="services">The service provider to use for accessing host services.</param>
        public AdapterHost(
            IEnumerable<IHostedService> hostedServices,
            ILogger<AdapterHost> logger,
            IHostEnvironment environment,
            IServiceProvider services
        )
        {
            Services = services;
            this.hostedServices = hostedServices;
            this.environment = environment;
            this.logger = logger;
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Starting. Environment: {@environment}", environment.EnvironmentName);

            if (environment.IsEnvironment("local"))
            {
                using var databaseContext = Services.GetRequiredService<DatabaseContext>();
                await databaseContext.Database.MigrateAsync(cancellationToken);
            }

            var tasks = from service in hostedServices select service.StartAsync(cancellationToken);
            await Task.WhenAll(tasks);

            var client = Services.GetRequiredService<IDiscordUserClient>();
            var channel = await client.CreateDirectMessageChannel(292780421911937024, cancellationToken);
            logger.LogCritical("Created channel: {@channel}", channel);

            logger.LogInformation("Started.");
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Stopping.");

            var tasks = from service in hostedServices select service.StopAsync(cancellationToken);
            await Task.WhenAll(tasks);

            logger.LogInformation("Stopped.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
