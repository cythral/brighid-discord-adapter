using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brighid.Discord.RestQueue
{
    /// <summary>
    /// The application host/runner.
    /// </summary>
    [LogCategory(LogCategoryName)]
    public class RestQueueHost : IHost
    {
        private const string LogCategoryName = "Rest Queue Host";
        private readonly ILogger<RestQueueHost> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestQueueHost" /> class.
        /// </summary>
        /// <param name="services">The service provider to use for accessing host services.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public RestQueueHost(IServiceProvider services, ILogger<RestQueueHost> logger)
        {
            Services = services;
            this.logger = logger;
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Starting.");
            await Task.CompletedTask;
            logger.LogInformation("Started.");
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Stopping.");
            await Task.CompletedTask;
            logger.LogInformation("Stopped.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
