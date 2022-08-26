using Brighid.Discord.Adapter.Management;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Management Service Collection Extensions.
    /// </summary>
    public static class ManagementServiceCollectionExtensions
    {
        /// <summary>
        /// Configures service collections with management services.
        /// </summary>
        /// <param name="services">Services to configure.</param>
        public static void ConfigureManagementServices(this IServiceCollection services)
        {
            services.AddSingleton<IAdapterContext, AdapterContext>();
            services.AddSingleton<IDnsService, DnsService>();
            services.UseBrighidIdentityWithHttp2<INodeService, NodeService>();
            services.UseBrighidIdentityWithHttp2<ITrafficShifter, TrafficShifter>();
        }
    }
}
