using System.Diagnostics.CodeAnalysis;
using System.IO;

using Brighid.Discord.Adapter.Gateway;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Brighid.Discord.Adapter
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
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Everything referenced in the loaded assembly is manually preserved in ILLink.Descriptors.xml")]
        public static void ConfigureGatewayServices(this IServiceCollection services, IConfiguration configuration)
        {
            var gatewayOptions = configuration.GetSection("Gateway");
            services.Configure<GatewayOptions>(gatewayOptions);
            services.AddSingleton<IGatewayRxWorker, DefaultGatewayRxWorker>();
            services.AddSingleton<IGatewayTxWorker, DefaultGatewayTxWorker>();
            services.AddSingleton<IGatewayUtilsFactory, DefaultGatewayUtilsFactory>();
            services.AddSingleton<IGatewayRestartService, DefaultGatewayRestartService>();
            services.AddSingleton<IGatewayMetadataService, DefaultGatewayMetadataService>();
            services.AddSingleton<IGatewayService, DefaultGatewayService>();
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<IGatewayService>());
            services.AddTransient(provider => new MemoryStream());

            services.UseBrighidIdentityLoginProviders();
        }
    }
}
