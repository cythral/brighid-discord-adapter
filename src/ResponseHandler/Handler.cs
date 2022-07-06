using System.Threading;
using System.Threading.Tasks;

using Amazon.SQS;
using Amazon.SQS.Model;

using Brighid.Discord.Serialization;

using Lambdajection.Attributes;
using Lambdajection.Sns;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    [SnsEventHandler(typeof(Startup))]
    public partial class Handler
    {
        private readonly IAmazonSQS sqs;
        private readonly ISerializer serializer;
        private readonly ISnsRecordMapper mapper;
        private readonly Options options;
        private readonly ILogger<Handler> logger;

        public Handler(
            IAmazonSQS sqs,
            ISerializer serializer,
            ISnsRecordMapper mapper,
            IOptions<Options> options,
            ILogger<Handler> logger
        )
        {
            this.sqs = sqs;
            this.serializer = serializer;
            this.mapper = mapper;
            this.options = options.Value;
            this.logger = logger;
        }

        public async Task<string> Handle(SnsMessage<string> @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mappedRequest = mapper.MapToRequest(@event);
            var message = serializer.Serialize(mappedRequest);
            var request = new SendMessageBatchRequest
            {
                QueueUrl = options.QueueUrl.ToString(),
                Entries = new()
                {
                    new SendMessageBatchRequestEntry
                    {
                        Id = mappedRequest.Id.ToString(),
                        MessageBody = message,
                    },
                },
            };

            logger.LogInformation("Sending sqs:SendMessageBatch with request: {@request}", request);
            var response = await sqs.SendMessageBatchAsync(request, cancellationToken);
            logger.LogInformation("Received sqs:SendMessageBatch response: {@response}", response);
            return "OK";
        }
    }
}
