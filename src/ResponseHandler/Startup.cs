using Amazon.SQS;

using Lambdajection.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    /// <inheritdoc />
    public class Startup : ILambdaStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            services.UseAwsService<IAmazonSQS>();
            services.AddSingleton<ISnsRecordMapper, SnsRecordMapper>();
        }
    }
}
