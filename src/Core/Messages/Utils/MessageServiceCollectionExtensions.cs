using System.Text.Json.Serialization;

using Brighid.Discord.Messages;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord
{
    /// <summary>
    /// Service collection extensions for Messages.
    /// </summary>
    public static class MessageServiceCollectionExtensions
    {
        /// <summary>
        /// Configures gateway services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The app configuration.</param>
        public static void ConfigureMessageServices(this IServiceCollection services, IConfiguration configuration)
        {
            var snsMessageEmitterOptions = configuration.GetSection("Sns");
            services.Configure<SnsMessageEmitterOptions>(snsMessageEmitterOptions);
            services.AddSingleton<IMessageEmitter, SnsMessageEmitter>();
            services.AddSingleton<JsonConverter<GatewayMessage>, GatewayMessageConverter>();
            services.AddSingleton<IMessageParser, GeneratedMessageParser>();
        }
    }
}
