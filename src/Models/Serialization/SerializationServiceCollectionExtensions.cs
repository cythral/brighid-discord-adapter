using System.Text.Json.Serialization;

using Brighid.Discord.Serialization;

namespace Microsoft.Extensions.DependencyInjection
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
        /// <param name="serializerContext">Context to use for serialization/deserialization.</param>
        public static void ConfigureSerializationServices(this IServiceCollection services, JsonSerializerContext serializerContext)
        {
            services.AddSingleton(serializerContext);
            services.AddSingleton<ISerializer, JsonSerializer>();
        }
    }
}
