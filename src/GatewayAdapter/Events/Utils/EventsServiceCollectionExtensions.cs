using Brighid.Discord.GatewayAdapter.Events;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord.GatewayAdapter
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
            services.AddSingleton<InvalidSessionEventController>();
            services.AddSingleton<MessageCreateEventController>();
            services.AddSingleton<ReadyEventController>();
            services.AddSingleton<ReconnectEventController>();
            services.AddSingleton<ResumedEventController>();
        }
    }
}
