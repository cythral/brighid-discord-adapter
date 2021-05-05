using System.Text.Json.Serialization;

using Brighid.Discord.GatewayAdapter.Messages;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using GatewayMessageConverter = Brighid.Discord.GatewayAdapter.Messages.GatewayMessageConverter;

namespace Brighid.Discord.GatewayAdapter
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
            services.AddSingleton<JsonConverter, GatewayMessageConverter>();
            services.AddSingleton<IMessageParser, GeneratedMessageParser>();
        }
    }
}
