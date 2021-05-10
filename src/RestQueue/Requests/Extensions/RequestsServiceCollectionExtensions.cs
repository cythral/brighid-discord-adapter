using System.Net.Http;

using Brighid.Discord.RestQueue.Requests;

using Microsoft.Extensions.Configuration;

using HttpClient = Brighid.Discord.RestQueue.Requests.HttpClient;

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
        public static void ConfigureRequestsServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RequestOptions>(configuration.GetSection("Requests"));
            services.AddSingleton<IUrlBuilder, DefaultUrlBuilder>();
            services.AddSingleton<IRequestMessageRelay, SqsRequestMessageRelay>();
            services.AddSingleton<IRequestInvoker, DefaultRequestInvoker>();
            services.AddSingleton<HttpMessageInvoker, HttpClient>();
        }
    }
}
