using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Gateway;
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
        private const string hostName = "Discord Adapter Host";
        private readonly ILogger<DiscordAdapterHost> logger;
        private IGatewayService? gateway;

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
            logger.LogInformation("{@hostName} Starting.", hostName);
            await Task.CompletedTask;

            Services.GetRequiredService<IMessageEmitter>();
            Services.GetRequiredService<ISerializer>();

            gateway = Services.GetRequiredService<IGatewayService>();

            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            gateway.Start(source);

            logger.LogInformation("{@hostName} Started.", hostName);

            // Tests:
            await gateway.Send(new GatewayMessage { OpCode = GatewayOpCode.Hello }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("{@hostName} Stopping.", hostName);
            await Task.CompletedTask;

            if (gateway != null)
            {
                gateway.Stop();
            }

            logger.LogInformation("{@hostName} Stopped.", hostName);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
