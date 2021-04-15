using Brighid.Discord.Serialization;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord
{
    /// <summary>
    /// Service collection extensions for Messages.
    /// </summary>
    public static class SerializationServiceCollectionExtensions
    {
        /// <summary>
        /// Configures gateway services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static void ConfigureSerializationServices(this IServiceCollection services)
        {
            services.AddSingleton<ISerializer, JsonSerializer>();
        }
    }
}
