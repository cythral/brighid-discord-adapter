using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.RestQueue.Requests;

using Microsoft.Extensions.DependencyInjection;
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

            var relay = Services.GetRequiredService<IRequestMessageRelay>();

            while (true)
            {
                var messages = await relay.Receive(cancellationToken);
                var tasks = from message in messages select relay.Complete(message, null, cancellationToken);

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception exception)
                {
                    logger.LogError("Received exception: {@exception}", exception);
                }

                await Task.Delay(30000, cancellationToken);
            }
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
