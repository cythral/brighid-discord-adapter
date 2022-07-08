using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Discord.Adapter.Management;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

#pragma warning disable SA1600, CS1591

namespace Brighid.Discord.Adapter
{
    public class Program
    {
        public static bool AutoDetectDatabaseVersion { get; private set; } = false;

        public static async Task Main(string[] args)
        {
            AutoDetectDatabaseVersion = true;
            using var cancellationTokenSource = new CancellationTokenSource();
            using var host = CreateHostBuilder(args).Build();

            var cancellationToken = cancellationTokenSource.Token;
            await FetchNodeInfo(host, cancellationToken);
            await host.RunAsync(cancellationToken);
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

        public static async Task FetchNodeInfo(IHost host, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nodeService = host.Services.GetRequiredService<INodeService>();
            var adapterContext = host.Services.GetRequiredService<IAdapterContext>();
            var ipAddress = await nodeService.GetIpAddress(cancellationToken);

            adapterContext.Set(new NodeInfo
            {
                IpAddress = ipAddress,
                Shard = 0,
                DeploymentId = await nodeService.GetDeploymentId(cancellationToken),
            });

            var peers = await nodeService.GetPeers(ipAddress, cancellationToken);
            adapterContext.Set(peers);
        }
    }
}
