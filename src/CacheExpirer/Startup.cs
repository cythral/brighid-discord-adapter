using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord.CacheExpirer
{
    public class Startup : ILambdaStartup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDnsService, DnsService>();

            services.ConfigureBrighidIdentity<IdentityOptions>(configuration.GetSection("Identity"));
            services.UseBrighidIdentityWithHttp2<ICacheService, CacheService>();
        }
    }
}
