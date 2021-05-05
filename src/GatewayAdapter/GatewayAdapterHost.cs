using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.GatewayAdapter.Gateway;
using Brighid.Discord.GatewayAdapter.Messages;
using Brighid.Discord.Serialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brighid.Discord.GatewayAdapter
{
    /// <summary>
    /// The application runner.
    /// </summary>
    [LogCategory(LogCategoryName)]
    public class GatewayAdapterHost : IHost
    {
        private const string LogCategoryName = "Gateway Adapter Host";
        private readonly ILogger<GatewayAdapterHost> logger;
        private IGatewayService? gateway;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatewayAdapterHost" /> class.
        /// </summary>
        /// <param name="services">The service provider to use for accessing host services.</param>
        public GatewayAdapterHost(IServiceProvider services)
        {
            Services = services;
            logger = services.GetRequiredService<ILogger<GatewayAdapterHost>>();
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Starting.");
            await Task.CompletedTask;

            Services.GetRequiredService<IMessageEmitter>();
            Services.GetRequiredService<ISerializer>();

            gateway = Services.GetRequiredService<IGatewayService>();

            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            gateway.Start(source);

            logger.LogInformation("Started.");

            // Tests:
            // await gateway.Send(new GatewayMessage { OpCode = GatewayOpCode.Hello }, cancellationToken);
            // await Task.Delay(5000);
            // await StopAsync();
            // await Task.Delay(1000);
            // await StartAsync();
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (gateway == null)
            {
                logger.LogInformation("Nothing to stop.");
                return;
            }

            await Task.CompletedTask;
            logger.LogInformation("Stopping.");
            gateway.Stop();
            logger.LogInformation("Stopped.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
