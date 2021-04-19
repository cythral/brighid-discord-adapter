using Brighid.Discord.Metrics;

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
        public static void ConfigureMetricServices(this IServiceCollection services, IConfiguration configuration)
        {
            var metricOptions = configuration.GetSection("Metrics");
            services.Configure<MetricOptions>(metricOptions);
            services.AddSingleton<IMetricReporter, CloudWatchMetricReporter>();
        }
    }
}
