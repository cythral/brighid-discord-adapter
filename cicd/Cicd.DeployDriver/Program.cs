using System.Collections.Generic;

using Brighid.Discord.Cicd.DeployDriver;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable SA1516

await Microsoft.Extensions.Hosting.Host
.CreateDefaultBuilder()
.ConfigureAppConfiguration(configure =>
{
    configure.AddCommandLine(args, new Dictionary<string, string>
    {
        ["--environment"] = "CommandLineOptions:Environment",
        ["--artifacts-location"] = "CommandLineOptions:ArtifactsLocation",
    });
})
.ConfigureServices((context, services) =>
{
    services.Configure<CommandLineOptions>(context.Configuration.GetSection("CommandLineOptions"));
    services.AddSingleton<IHost, Brighid.Discord.Cicd.DeployDriver.Host>();
    services.AddSingleton<StackDeployer>();
})
.UseConsoleLifetime()
.Build()
.RunAsync();
