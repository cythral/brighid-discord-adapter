using System.Threading;
using System.Threading.Tasks;

using Amazon.SQS;

using Brighid.Discord.Models;
using Brighid.Discord.Serialization;

using Microsoft.Extensions.Options;

namespace Brighid.Discord.RestClient.Client
{
    /// <inheritdoc />
    /// <todo>Use Batching.</todo>
    public class SqsRequestQueuer : IRequestQueuer
    {
        private readonly IAmazonSQS sqsClient;
        private readonly ISerializer serializer;
        private readonly ClientOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqsRequestQueuer"/> class.
        /// </summary>
        /// <param name="sqsClient">SQS Client to use.</param>
        /// <param name="serializer">Serializer used to serialize/deserialize data between formats.</param>
        /// <param name="options">Options to use when making requests.</param>
        public SqsRequestQueuer(
            IAmazonSQS sqsClient,
            ISerializer serializer,
            IOptions<ClientOptions> options
        )
        {
            this.sqsClient = sqsClient;
            this.serializer = serializer;
            this.options = options.Value;
        }

        /// <inheritdoc />
        public async Task QueueRequest(Request request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var message = await serializer.Serialize(request, cancellationToken);
            await sqsClient.SendMessageAsync(options.RequestQueueUrl, message, cancellationToken);
        }
    }
}
