using Brighid.Discord.RestClient.Responses;

using Microsoft.Extensions.DependencyInjection.Extensions;

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
        public static void ConfigureResponseServices(this IServiceCollection services)
        {
            services.TryAddSingleton<ITcpListener, DefaultTcpListener>();
        }
    }
}
