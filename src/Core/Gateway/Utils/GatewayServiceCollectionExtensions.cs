using System.IO;

using Brighid.Discord.Gateway;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord
{
    /// <summary>
    /// Service collection extensions for the Gateway.
    /// </summary>
    public static class GatewayServiceCollectionExtensions
    {
        /// <summary>
        /// Configures gateway services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The app configuration.</param>
        public static void ConfigureGatewayServices(this IServiceCollection services, IConfiguration configuration)
        {
            var gatewayOptions = configuration.GetSection("Gateway");
            services.Configure<GatewayOptions>(gatewayOptions);
            services.AddSingleton<IGatewayRxWorker, DefaultGatewayRxWorker>();
            services.AddSingleton<IGatewayTxWorker, DefaultGatewayTxWorker>();
            services.AddSingleton<IGatewayService, DefaultGatewayService>();
            services.AddSingleton<IGatewayUtilsFactory, DefaultGatewayUtilsFactory>();
            services.AddSingleton<IGatewayRestartService, DefaultGatewayRestartService>();
            services.AddTransient(provider => new MemoryStream());
        }
    }
}
