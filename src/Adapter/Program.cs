﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Amazon.ECS;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

using Brighid.Discord.Adapter;
using Brighid.Discord.Adapter.Database;
using Brighid.Discord.Adapter.Management;
using Brighid.Discord.Tracing;

using Destructurama;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

using Environments = Brighid.Discord.Adapter.Environments;
using JsonSerializer = Brighid.Discord.Serialization.JsonSerializer;

#pragma warning disable SA1516

using var cancellationTokenSource = new CancellationTokenSource();
var serializer = new JsonSerializer(JsonContext.Default);
var cancellationToken = cancellationTokenSource.Token;
var environment = Environment.GetEnvironmentVariable("Environment") ?? Environments.Local;
var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, EnvironmentName = environment });

builder.Configuration.AddEnvironmentVariables();
builder.Host.UseSerilog(dispose: true);
builder.Host.UseDefaultServiceProvider((context, provider) =>
{
#pragma warning disable IL2026
    var validateScopes = context.Configuration.GetValue<bool>("Adapter:ValidateScopes");
    provider.ValidateOnBuild = validateScopes;
    provider.ValidateScopes = validateScopes;
#pragma warning restore IL2026
});

builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.ListenAnyIP(80, listener => listener.Protocols = HttpProtocols.Http2);
});

builder.Services.ConfigureTracingServices();
AWSSDKHandler.RegisterXRayForAllServices();

builder.Services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
builder.Services.AddSingleton<IAmazonECS, AmazonECSClient>();
builder.Services.AddSingleton<IAmazonSQS, AmazonSQSClient>();

builder.Services
.AddControllers(options => options.InputFormatters.Add(new EnumTextFormatter()))
.AddControllersAsServices();

builder.Services.AddHealthChecks();
builder.Services.AddLocalization(options => options.ResourcesPath = string.Empty);

#pragma warning disable IL2026, IL2062
var adapterOptionsSection = builder.Configuration.GetSection("Adapter");
builder.Services.Configure<AdapterOptions>(adapterOptionsSection);
builder.Services.TryAddSingleton<Random>();
builder.Services.AddSingleton(typeof(ILogger<>), typeof(Brighid.Discord.Logger<>));

builder.Services.ConfigureBrighidIdentity<IdentityOptions>(builder.Configuration.GetSection("Identity"));
builder.Services.AddBrighidCommands(builder.Configuration.GetSection("Commands").Bind);
#pragma warning restore IL2026

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("2600:1f18:22e4:7b00::"), 56));
    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("2600:1f18:2323:b900::"), 56));
    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("2600:1f18:24a8:e000::"), 56));
});

builder.Services.AddRazorPages();
builder.Services.ConfigureSerializationServices(JsonContext.Default);
builder.Services.ConfigureThreadingServices();
builder.Services.ConfigureManagementServices();
builder.Services.ConfigureDependencyInjectionServices();
builder.Services.ConfigureEventsServices();
builder.Services.ConfigureUsersServices();
builder.Services.ConfigureDatabaseServices(builder.Configuration);
builder.Services.ConfigureRequestsServices(builder.Configuration);
builder.Services.ConfigureGatewayServices(builder.Configuration);
builder.Services.ConfigureMessageServices(builder.Configuration);
builder.Services.ConfigureRestClientResponseServices();
builder.Services.ConfigureRestClientServices(builder.Configuration);

#pragma warning disable IL2026
builder.Services.ConfigureAuthServices(builder.Configuration.GetSection("Auth").Bind);
#pragma warning restore IL2026

using var host = builder.Build();
host.UseExceptionHandler(handler =>
{
    handler.Run(context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var feature = context.Features.GetRequiredFeature<IExceptionHandlerPathFeature>();
        logger.LogError(feature.Error, "Caught exception when executing API endpoint.");
        return Task.CompletedTask;
    });
});

host.UseForwardedHeaders();
host.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider("/app/wwwroot"),
    ServeUnknownFileTypes = true,
});
host.MapHealthChecks("/healthcheck");

host.UseAuthentication();
host.UseRouting();
host.UseAuthorization();
host.MapControllers();

SetupLogger(host);

await FetchNodeInfo(host, cancellationToken);
await InitializeDatabase(host, cancellationToken);
await host.RunAsync();

static void SetupLogger(WebApplication host)
{
    var adapterOptions = host.Services.GetRequiredService<IOptions<AdapterOptions>>();
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(host.Configuration)
        .Destructure.UsingAttributes()
        .Enrich.FromLogContext()
        .MinimumLevel.Is(adapterOptions.Value.LogLevel)
        .MinimumLevel.Override("DefaultHealthCheckService", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore.Server.Kestrel", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
        .Filter.ByExcluding("RequestPath = '/healthcheck' and (StatusCode = 200 or EventId.Name = 'ExecutingEndpoint' or EventId.Name = 'ExecutedEndpoint')")
        .WriteTo.Console(formatter: new JsonFormatter())
        .CreateLogger();
}

static async Task FetchNodeInfo(IHost host, CancellationToken cancellationToken)
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

static async Task InitializeDatabase(IHost host, CancellationToken cancellationToken)
{
    await using var scope = host.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    await database.Database.OpenConnectionAsync(cancellationToken);

    var databaseOptions = host.Services.GetRequiredService<IOptions<DatabaseOptions>>();
    if (databaseOptions.Value.RunMigrationsOnStartup)
    {
        await database.Database.MigrateAsync(cancellationToken);
    }
}
