using Amazon.SimpleNotificationService;

using Brighid.Discord.Gateway;
using Brighid.Discord.Messages;
using Brighid.Discord.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord
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
        }

        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
            services.AddSingleton<ISerializer, JsonSerializer>();

            var gatewayOptions = configuration.GetSection("Gateway");
            services.Configure<GatewayOptions>(gatewayOptions);
            services.AddSingleton<IGatewayWorker, DefaultGatewayWorker>();
            services.AddSingleton<IGatewayService, DefaultGatewayService>();
            ConfigureMessageServices(services);
        }

        private void ConfigureMessageServices(IServiceCollection services)
        {
            var snsMessageEmitterOptions = configuration.GetSection("Sns");
            services.Configure<SnsMessageEmitterOptions>(snsMessageEmitterOptions);
            services.AddSingleton<IMessageEmitter, SnsMessageEmitter>();
            services.AddSingleton<IMessageParser, GeneratedMessageParser>();
        }
    }
}
