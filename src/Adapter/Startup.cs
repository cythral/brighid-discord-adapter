using System;

using Amazon.CloudWatch;
using Amazon.SimpleNotificationService;
using Amazon.SQS;

using Brighid.Discord.Adapter.Requests;

using Destructurama;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

namespace Brighid.Discord.Adapter
{
    /// <inheritdoc />
    public class Startup : IStartup
    {
        private readonly IConfiguration configuration;
        private readonly DatabaseOptions databaseOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="configuration">Configuration to use for the application.</param>
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
            databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>();

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

            services.AddDbContextPool<DatabaseContext>(ConfigureDatabaseOptions);
            services.ConfigureBrighidIdentity("Identity");
            services.ConfigureSerializationServices();
            services.ConfigureThreadingServices();
            services.ConfigureDependencyInjectionServices();
            services.ConfigureEventsServices();
            services.ConfigureUsersServices();
            services.ConfigureRequestsServices(configuration);
            services.ConfigureGatewayServices(configuration);
            services.ConfigureMessageServices(configuration);
            services.ConfigureMetricServices(configuration);
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
            services.AddSingleton<Random>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }

        private void ConfigureDatabaseOptions(DbContextOptionsBuilder options)
        {
            var conn = $"Server={databaseOptions.Host};";
            conn += $"Database={databaseOptions.Name};";
            conn += $"User={databaseOptions.User};";
            conn += $"Password=\"{databaseOptions.Password}\";";
            conn += "GuidFormat=Binary16";

            options
            .UseMySql(
                conn,
                new MySqlServerVersion(new Version(5, 7, 0))
            );
        }
    }
}
