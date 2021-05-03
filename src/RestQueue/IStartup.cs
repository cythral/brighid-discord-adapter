using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord.RestQueue
{
    /// <summary>
    /// Configures an application on startup.
    /// </summary>
    public interface IStartup
    {
        /// <summary>
        /// Configures services for the IoC Container.
        /// </summary>
        /// <param name="services">The collection to add services to.</param>
        void ConfigureServices(IServiceCollection services);
    }
}
