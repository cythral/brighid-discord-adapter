using System.Net.Http;

using Lambdajection.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord.AlarmNotifier
{
    public class Startup : ILambdaStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();
        }
    }
}
