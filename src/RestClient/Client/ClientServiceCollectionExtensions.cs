using Amazon.SQS;

using Brighid.Discord.RestClient.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Service collection extensions for Messages.
    /// </summary>
    public static class ClientServiceCollectionExtensions
    {
        /// <summary>
        /// Configures gateway services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">Configuration to use for the services.</param>
        public static void ConfigureRestClientServices(this IServiceCollection services, IConfiguration configuration)
        {
#pragma warning disable IL2026
            services.Configure<ClientOptions>(configuration.GetSection("RestClient"));
#pragma warning restore IL2026

            services.TryAddSingleton<IRequestQueuer, SqsRequestQueuer>();
            services.TryAddSingleton<IAmazonSQS, AmazonSQSClient>();
            services.TryAddSingleton<IDiscordUserClient, DefaultDiscordUserClient>();
            services.TryAddSingleton<IDiscordChannelClient, DefaultDiscordChannelClient>();
            services.TryAddSingleton<IDiscordGatewayClient, DefaultDiscordGatewayClient>();
            services.TryAddSingleton<IDiscordRequestHandler, DefaultDiscordRequestHandler>();
        }
    }
}
