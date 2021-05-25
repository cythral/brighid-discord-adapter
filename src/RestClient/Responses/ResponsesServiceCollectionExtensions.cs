using Brighid.Discord.RestClient.Responses;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Service collection extensions for Messages.
    /// </summary>
    public static class ResponsesServiceCollectionExtensions
    {
        /// <summary>
        /// Configures gateway services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static void ConfigureRestClientResponseServices(this IServiceCollection services)
        {
            services.ConfigureNetworkingServices();
            services.ConfigureThreadingServices();
            services.TryAddSingleton<ITcpListener, DefaultTcpListener>();
            services.TryAddSingleton<IRequestMap, DefaultRequestMap>();
            services.TryAddSingleton<IResponseServer, DefaultResponseServer>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<IResponseServer>());
        }
    }
}
