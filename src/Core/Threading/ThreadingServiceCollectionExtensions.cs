using System;

using Brighid.Discord.Threading;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Service collection extensions for Messages.
    /// </summary>
    public static class ThreadingServiceCollectionExtensions
    {
        /// <summary>
        /// Configures gateway services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static void ConfigureThreadingServices(this IServiceCollection services)
        {
            services.TryAddSingleton<Random>();
            services.AddSingleton<ITimerFactory, DefaultTimerFactory>();
            services.AddSingleton(typeof(IChannel<>), typeof(Channel<>));
        }
    }
}
