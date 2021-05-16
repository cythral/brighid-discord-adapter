using Brighid.Discord.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Service collection extensions for Messages.
    /// </summary>
    public static class DependencyInjectionServiceCollectionExtensions
    {
        /// <summary>
        /// Configures gateway services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static void ConfigureDependencyInjectionServices(this IServiceCollection services)
        {
            services.AddSingleton<IScopeFactory, DefaultScopeFactory>();
        }
    }
}
