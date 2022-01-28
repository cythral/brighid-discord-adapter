using System;
using System.Diagnostics.CodeAnalysis;

using Brighid.Discord.Adapter;
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
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Everything referenced is preserved via attributes.")]
        public static void ConfigureDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>() ?? new();
            services.AddDbContext<DatabaseContext>(options =>
            {
                var conn = $"Server={databaseOptions.Host};";
                conn += $"Database={databaseOptions.Name};";
                conn += $"User={databaseOptions.User};";
                conn += $"Password=\"{databaseOptions.Password}\";";
                conn += "GuidFormat=Binary16;";
                conn += "DefaultCommandTimeout=0;";
                conn += "UseCompression=true";

                var version = Program.AutoDetectDatabaseVersion
                    ? ServerVersion.AutoDetect(conn)
                    : new MySqlServerVersion(new Version(5, 7, 0));

                options.UseMySql(conn, version);
            });

            services.AddSingleton<ITransactionFactory, DefaultTransactionFactory>();
        }
    }
}
