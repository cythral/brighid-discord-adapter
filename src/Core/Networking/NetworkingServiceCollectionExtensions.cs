using Brighid.Discord.Networking;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Service collection extensions for Messages.
    /// </summary>
    public static class NetworkingServiceCollectionExtensions
    {
        /// <summary>
        /// Configures gateway services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static void ConfigureNetworkingServices(this IServiceCollection services)
        {
            services.TryAddSingleton<ITcpClientFactory, DefaultTcpClientFactory>();
        }
    }
}
