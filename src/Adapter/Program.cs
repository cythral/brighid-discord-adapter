using System;
using System.Net;
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

#pragma warning disable IDE0078 // Use Pattern Matching
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("Environment") ?? Environments.Local;
            var isDevOrLocal = environment == Environments.Local || environment == Environments.Development;

            return Host.CreateDefaultBuilder(args)
                .UseEnvironment(environment)
                .UseSerilog(dispose: true)
                .ConfigureHostConfiguration(config =>
                {
                    config.AddEnvironmentVariables();
                })
                .UseDefaultServiceProvider(options =>
                {
                    options.ValidateScopes = isDevOrLocal;
                    options.ValidateOnBuild = isDevOrLocal;
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseStartup<Startup>();
                    builder.ConfigureKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, 80, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                        });
                    });
                });
        }
#pragma warning restore IDE0078
    }
}
