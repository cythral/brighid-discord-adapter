using System.Diagnostics.CodeAnalysis;

using Brighid.Discord.Adapter.Metrics;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord
{
    /// <summary>
    /// Service collection extensions for Metrics.
    /// </summary>
    public static class MetricsServiceCollectionExtensions
    {
        /// <summary>
        /// Configures metric services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The app configuration.</param>
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Everything referenced in the loaded assembly is manually preserved in ILLink.Descriptors.xml")]
        public static void ConfigureMetricServices(this IServiceCollection services, IConfiguration configuration)
        {
            var metricOptions = configuration.GetSection("Metrics");
            services.Configure<MetricOptions>(metricOptions);
            services.AddSingleton<IMetricReporter, CloudWatchMetricReporter>();
        }
    }
}
