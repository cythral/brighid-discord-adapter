using System.Net.Http;

using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.System.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Brighid.Discord.Tracing
{
    /// <summary>
    /// Service collection extensions for tracing.
    /// </summary>
    public static class TracingServiceCollectionExtensions
    {
        /// <summary>
        /// Configures tracing services on the service collection.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public static void ConfigureTracingServices(this IServiceCollection services)
        {
            AWSXRayRecorder.InitializeInstance();

            services.AddTransient<DelegatingHandler, HttpClientXRayTracingHandler>();
            services.TryAddSingleton<ITracingService, AwsXRayTracingService>();
            services.TryAddSingleton<ITracingIdService, AwsXRayTracingIdService>();
            services.TryAddSingleton<IAWSXRayRecorder>(AWSXRayRecorder.Instance);
        }
    }
}
