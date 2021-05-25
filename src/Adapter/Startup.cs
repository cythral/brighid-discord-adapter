using System;

using Amazon.CloudWatch;
using Amazon.SimpleNotificationService;
using Amazon.SQS;

using Destructurama;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using Serilog;

namespace Brighid.Discord.Adapter
{
    /// <inheritdoc />
    public class Startup : IStartup
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="configuration">Configuration to use for the application.</param>
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Destructure.UsingAttributes()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:s}] {Message:lj} {Properties:j} {Exception}{NewLine}")
                .CreateLogger();
        }

        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureAwsServices(services);
            ConfigureMiscServices(services);

            services.ConfigureBrighidIdentity("Identity");
            services.ConfigureSerializationServices();
            services.ConfigureThreadingServices();
            services.ConfigureDependencyInjectionServices();
            services.ConfigureEventsServices();
            services.ConfigureUsersServices();
            services.ConfigureDatabaseServices(configuration);
            services.ConfigureRequestsServices(configuration);
            services.ConfigureGatewayServices(configuration);
            services.ConfigureMessageServices(configuration);
            services.ConfigureMetricServices(configuration);

            services.ConfigureRestClientResponseServices();
            services.ConfigureRestClientServices(configuration);
        }

        private static void ConfigureAwsServices(IServiceCollection services)
        {
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
            services.AddSingleton<IAmazonCloudWatch, AmazonCloudWatchClient>();
            services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
        }

        private void ConfigureMiscServices(IServiceCollection services)
        {
            services.Configure<AdapterOptions>(configuration.GetSection("Adapter"));
            services.TryAddSingleton<Random>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }
    }
}
