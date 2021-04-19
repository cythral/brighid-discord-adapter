using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

#pragma warning disable SA1600, CS1591

namespace Brighid.Discord
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IStartup CreateStartup(IConfiguration configuration)
        {
            return new Startup(configuration);
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog(dispose: true)
                .ConfigureServices((context, services) =>
                {
                    CreateStartup(context.Configuration).ConfigureServices(services);
                    services.AddSingleton<IHost, DiscordAdapterHost>();
                });
        }
    }
}
