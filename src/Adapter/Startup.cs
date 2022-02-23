using System;
using System.Diagnostics.CodeAnalysis;

using Amazon.CloudWatch;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

using Brighid.Discord.Adapter.Database;
using Brighid.Discord.Tracing;

using Destructurama;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
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
        /// <param name="environment">Environment to use for the program.</param>
        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment environment
        )
        {
            this.configuration = configuration;
            var loggerBuilder = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Destructure.UsingAttributes()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:s}] {Message:lj} {Properties:j} {Exception}{NewLine}");

            loggerBuilder = environment.EnvironmentName switch
            {
                Environments.Local or Environments.Development => loggerBuilder.MinimumLevel.Debug(),
                _ => loggerBuilder.MinimumLevel.Information(),
            };

            Log.Logger = loggerBuilder.CreateLogger();
        }

        /// <inheritdoc />
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Everything referenced is preserved via attributes.")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureTracingServices();

            ConfigureAwsServices(services);
            ConfigureMiscServices(services);
            ConfigureBrighidServices(services);

            services.Configure<ForwardedHeadersOptions>(ConfigureForwardedHeadersOptions);
            services.AddRazorPages();
            services.ConfigureSerializationServices(JsonContext.Default);
            services.ConfigureThreadingServices();
            services.ConfigureDependencyInjectionServices();
            services.ConfigureEventsServices();
            services.ConfigureUsersServices();
            services.ConfigureDatabaseServices(configuration);
            services.ConfigureRequestsServices(configuration);
            services.ConfigureGatewayServices(configuration);
            services.ConfigureMessageServices(configuration);
            services.ConfigureMetricServices(configuration);
            services.ConfigureAuthServices(configuration.GetSection("Auth").Bind);
            services.ConfigureRestClientResponseServices();
            services.ConfigureRestClientServices(configuration);
        }

        /// <summary>
        /// Configures the environment.
        /// </summary>
        /// <param name="app">The application being configured.</param>
        /// <param name="environment">Environment used for the adapter.</param>
        /// <param name="tracing">Service used for managing application traces.</param>
        /// <param name="databaseContext">Context used to interact with the database.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public void Configure(
            IApplicationBuilder app,
            IHostEnvironment environment,
            ITracingService tracing,
            DatabaseContext databaseContext,
            ILogger<Startup> logger
        )
        {
            logger.LogInformation("Starting. Environment: {@environment}", environment.EnvironmentName);
            databaseContext.Database.OpenConnection();

            if (environment.IsEnvironment(Environments.Local))
            {
                using var trace = tracing.StartTrace();
                databaseContext.Database.Migrate();
            }

            app.UseForwardedHeaders();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider("/app/wwwroot"),
                ServeUnknownFileTypes = true,
            });

            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthcheck");
                endpoints.MapControllers();
            });
        }

        private static void ConfigureForwardedHeadersOptions(ForwardedHeadersOptions options)
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        }

        private static void ConfigureAwsServices(IServiceCollection services)
        {
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
            AWSSDKHandler.RegisterXRay<IAmazonSimpleNotificationService>();

            services.AddSingleton<IAmazonCloudWatch, AmazonCloudWatchClient>();
            AWSSDKHandler.RegisterXRay<IAmazonCloudWatch>();

            services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
        }

        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Everything referenced is preserved via attributes.")]
        private void ConfigureBrighidServices(IServiceCollection services)
        {
            services.ConfigureBrighidIdentity<IdentityOptions>(configuration.GetSection("Identity"));
            services.AddBrighidCommands(configuration.GetSection("Commands").Bind);
        }

        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Everything referenced is preserved via attributes.")]
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
