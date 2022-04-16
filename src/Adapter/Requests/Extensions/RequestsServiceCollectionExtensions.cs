using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

using Brighid.Discord.Adapter.Requests;

using Microsoft.Extensions.Configuration;

using HttpClient = Brighid.Discord.Adapter.Requests.HttpClient;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IServiceCollection Extensions for Requests.
    /// </summary>
    public static class RequestsServiceCollectionExtensions
    {
        /// <summary>
        /// Configures the service collection to use request services.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">Application configuration object.</param>
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Everything referenced in the loaded assembly is manually preserved in ILLink.Descriptors.xml")]
        public static void ConfigureRequestsServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RequestOptions>(configuration.GetSection("Requests"));
            services.AddSingleton<IUrlBuilder, DefaultUrlBuilder>();
            services.AddSingleton<IRequestMessageRelay, SqsRequestMessageRelay>();
            services.AddScoped<IRequestInvoker, DefaultRequestInvoker>();
            services.AddScoped<IBucketService, DefaultBucketService>();
            services.AddScoped<IBucketRepository, DefaultBucketRepository>();
            services.AddSingleton<HttpMessageInvoker, HttpClient>();

            services.AddHostedService<DefaultRequestWorker>();
            services.UseBrighidIdentityWithHttp2<IRequestMessageRelayHttpClient, DefaultRequestMessageRelayHttpClient>();
        }
    }
}
