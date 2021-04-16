using Brighid.Discord.Events;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord
{
    /// <summary>
    /// Service collection extensions for the Gateway.
    /// </summary>
    public static class EventsServiceCollectionExtensions
    {
        /// <summary>
        /// Configures gateway services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static void ConfigureEventsServices(this IServiceCollection services)
        {
            services.AddSingleton<IEventRouter, GeneratedEventRouter>();
            services.AddSingleton<HelloEventController>();
        }
    }
}
