using Amazon.SQS;

using Brighid.Discord.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

namespace Brighid.Discord.RestQueue
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
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:s}] {Message:lj} {Properties:j} {Exception}{NewLine}")
                .CreateLogger();
        }

        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureMiscServices(services);
            services.ConfigureRequestsServices(configuration);
            services.ConfigureSerializationServices();
        }

        private static void ConfigureMiscServices(IServiceCollection services)
        {
            services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddTransient(typeof(IChannel<>), typeof(Channel<>));
        }
    }
}
