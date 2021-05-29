using System;

using Amazon.CloudWatch;
using Amazon.SimpleNotificationService;
using Amazon.SQS;

using Brighid.Discord.Adapter.Database;

using Destructurama;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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

            services.ConfigureBrighidIdentity(configuration.GetSection("Identity"));
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

            services.ConfigureRestClientResponseServices(configuration);
            services.ConfigureRestClientServices(configuration);
        }

        /// <summary>
        /// Configures the environment.
        /// </summary>
        /// <param name="app">The application being configured.</param>
        /// <param name="environment">Environment used for the adapter.</param>
        /// <param name="databaseContext">Context used to interact with the database.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public void Configure(
            IApplicationBuilder app,
            IHostEnvironment environment,
            DatabaseContext databaseContext,
            ILogger<Startup> logger
        )
        {
            logger.LogInformation("Starting. Environment: {@environment}", environment.EnvironmentName);

            if (environment.IsEnvironment("local"))
            {
                databaseContext.Database.MigrateAsync().GetAwaiter().GetResult();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthcheck");
                endpoints.MapControllers();
            });
        }

        private static void ConfigureAwsServices(IServiceCollection services)
        {
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
            services.AddSingleton<IAmazonCloudWatch, AmazonCloudWatchClient>();
            services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
        }

        private void ConfigureMiscServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks();
            services.AddLocalization(options => options.ResourcesPath = string.Empty);
            services.Configure<AdapterOptions>(configuration.GetSection("Adapter"));
            services.TryAddSingleton<Random>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }
    }
}
