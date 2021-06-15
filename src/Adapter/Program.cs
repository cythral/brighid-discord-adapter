using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;

#pragma warning disable SA1600, CS1591

namespace Brighid.Discord.Adapter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .UseSerilog(dispose: true)
            .ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables();
            })
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<Startup>();
                builder.UseUrls("http://0.0.0.0:80");
            });
        }
    }
}
