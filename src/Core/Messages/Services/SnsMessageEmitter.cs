using System.Threading;
using System.Threading.Tasks;

using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

using Brighid.Discord.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Messages
{
    /// <summary>
    /// An object that can emit messages to SNS.
    /// </summary>
    public class SnsMessageEmitter : IMessageEmitter
    {
        private readonly IAmazonSimpleNotificationService snsClient;

        private readonly SnsMessageEmitterOptions options;

        private readonly ISerializer serializer;

        private readonly ILogger<SnsMessageEmitter> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnsMessageEmitter" /> class.
        /// </summary>
        /// <param name="snsClient">Client used for interacting with the SNS API.</param>
        /// <param name="serializer">The serializer to use when serializing messages.</param>
        /// <param name="options">Options to use for the sns message emitter.</param>
        /// <param name="logger">The logger to use.</param>
        public SnsMessageEmitter(
            IAmazonSimpleNotificationService snsClient,
            ISerializer serializer,
            IOptions<SnsMessageEmitterOptions> options,
            ILogger<SnsMessageEmitter> logger
        )
        {
            this.snsClient = snsClient;
            this.serializer = serializer;
            this.options = options.Value;
            this.logger = logger;
        }

        /// <summary>
        /// Emits a message to SNS.
        /// </summary>
        /// <typeparam name="TMessageType">The type of message to emit.</typeparam>
        /// <param name="message">The message to emit.</param>
        /// <param name="cancellationToken">Token used to cancel the task.</param>
        /// <returns>The resulting task.</returns>
        public async Task Emit<TMessageType>(TMessageType message, CancellationToken cancellationToken = default)
        {
            var serializedMessage = await serializer.Serialize(message, cancellationToken);
            var request = new PublishRequest { TopicArn = options.TopicArn, Message = serializedMessage };
            logger.LogInformation("Sending sns:PublishAsync with request: {@request}", request);

            var response = await snsClient.PublishAsync(request, cancellationToken);
            logger.LogInformation("Received sns:PublishAsync response: {@response}", response);
        }
    }
}
