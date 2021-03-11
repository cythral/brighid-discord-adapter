using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Messages;
using Brighid.Discord.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brighid.Discord
{
    /// <summary>
    /// The application runner.
    /// </summary>
    public class DiscordAdapterHost : IHost
    {
        private readonly ILogger<DiscordAdapterHost> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordAdapterHost" /> class.
        /// </summary>
        /// <param name="services">The service provider to use for accessing host services.</param>
        public DiscordAdapterHost(IServiceProvider services)
        {
            Services = services;
            logger = services.GetRequiredService<ILogger<DiscordAdapterHost>>();
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Started DiscordAdapterHost.");
            Services.GetRequiredService<IMessageEmitter>();
            Services.GetRequiredService<ISerializer>();
            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Started DiscordAdapterHost.");
            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
