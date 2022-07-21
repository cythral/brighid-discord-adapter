using System;
using System.Threading;
using System.Threading.Tasks;

using Amazon.CloudWatch;
using Amazon.ECS;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

using Brighid.Discord;
using Brighid.Discord.Adapter;
using Brighid.Discord.Adapter.Database;
using Brighid.Discord.Adapter.Management;
using Brighid.Discord.Tracing;

using Destructurama;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Serilog;
using Serilog.Events;

using Environments = Brighid.Discord.Adapter.Environments;

#pragma warning disable SA1516

using var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;
var environment = Environment.GetEnvironmentVariable("Environment") ?? Environments.Local;
var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, EnvironmentName = environment });

builder.Configuration.AddEnvironmentVariables();
builder.Host.UseSerilog(dispose: true);
builder.Host.UseDefaultServiceProvider((context, provider) =>
{
    var validateScopes = context.Configuration.GetValue<bool>("Adapter:ValidateScopes");
    provider.ValidateOnBuild = validateScopes;
    provider.ValidateScopes = validateScopes;
});

builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.ListenAnyIP(80, listener => listener.Protocols = HttpProtocols.Http2);
});

builder.Services.ConfigureTracingServices();
AWSSDKHandler.RegisterXRayForAllServices();

builder.Services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
builder.Services.AddSingleton<IAmazonCloudWatch, AmazonCloudWatchClient>();
builder.Services.AddSingleton<IAmazonECS, AmazonECSClient>();
builder.Services.AddSingleton<IAmazonSQS, AmazonSQSClient>();

builder.Services
.AddControllers(options => options.InputFormatters.Add(new EnumTextFormatter()))
.AddControllersAsServices();

builder.Services.AddHealthChecks();
builder.Services.AddLocalization(options => options.ResourcesPath = string.Empty);
builder.Services.Configure<AdapterOptions>(builder.Configuration.GetSection("Adapter"));
builder.Services.TryAddSingleton<Random>();
builder.Services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(Logger<>));

builder.Services.ConfigureBrighidIdentity<IdentityOptions>(builder.Configuration.GetSection("Identity"));
builder.Services.AddBrighidCommands(builder.Configuration.GetSection("Commands").Bind);

builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
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
builder.Services.ConfigureMetricServices(builder.Configuration);
builder.Services.ConfigureAuthServices(builder.Configuration.GetSection("Auth").Bind);
builder.Services.ConfigureRestClientResponseServices();
builder.Services.ConfigureRestClientServices(builder.Configuration);

using var host = builder.Build();
host.UseForwardedHeaders();
host.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider("/app/wwwroot"),
    ServeUnknownFileTypes = true,
});

host.UseAuthentication();
host.UseRouting();
host.UseAuthorization();
host.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/healthcheck");
    endpoints.MapControllers();
});

SetupLogger(host);
await FetchNodeInfo(host, cancellationToken);
await InitializeDatabase(host, cancellationToken);
await host.RunAsync(cancellationToken);

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
        .WriteTo.Console(outputTemplate: "[{Timestamp:u}] [{Level:u3}] [{SourceContext:s}] {Message:lj} {Properties:j} {Exception}{NewLine}")
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
        database.Database.Migrate();
    }
}
