using System;

using Brighid.Discord.Adapter.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IServiceCollection Extensions for Database.
    /// </summary>
    public static class DatabaseServiceCollectionExtensions
    {
        /// <summary>
        /// Configures the service collection to use request services.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">Application configuration object.</param>
        public static void ConfigureDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>();
            services.AddDbContext<DatabaseContext>(options =>
            {
                var conn = $"Server={databaseOptions.Host};";
                conn += $"Database={databaseOptions.Name};";
                conn += $"User={databaseOptions.User};";
                conn += $"Password=\"{databaseOptions.Password}\";";
                conn += "GuidFormat=Binary16;";
                conn += "DefaultCommandTimeout=0";

                options
                .UseMySql(
                    conn,
                    new MySqlServerVersion(new Version(5, 7, 7))
                );
            });

            services.AddSingleton<ITransactionFactory, DefaultTransactionFactory>();
        }
    }
}
