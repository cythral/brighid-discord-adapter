using Brighid.Discord.Users;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord
{
    /// <summary>
    /// Service collection extensions for the Gateway.
    /// </summary>
    public static class UsersServiceCollectionExtensions
    {
        /// <summary>
        /// Configures gateway services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static void ConfigureUsersServices(this IServiceCollection services)
        {
            services.AddSingleton<IUserIdCache, ConcurrentUserIdCache>();
            services.AddSingleton<IUserService, DefaultUserService>();
            services.UseBrighidIdentityLoginProviders();
        }
    }
}
