using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.SNSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;

using Lambdajection.Attributes;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        private readonly IAmazonSQS sqs;
        private readonly ISnsRecordMapper mapper;
        private readonly Options options;
        private readonly ILogger<Handler> logger;

        public Handler(
            IAmazonSQS sqs,
            ISnsRecordMapper mapper,
            IOptions<Options> options,
            ILogger<Handler> logger
        )
        {
            this.sqs = sqs;
            this.mapper = mapper;
            this.options = options.Value;
            this.logger = logger;
        }

        public async Task<bool> Handle(SNSEvent @event, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entries = from record in @event.Records
                          let mappedRequest = mapper.MapToRequest(record)
                          let message = JsonSerializer.Serialize(mappedRequest)
                          select new SendMessageBatchRequestEntry
                          {
                              Id = mappedRequest.Id.ToString(),
                              MessageBody = message,
                          };

            var request = new SendMessageBatchRequest
            {
                QueueUrl = options.QueueUrl.ToString(),
                Entries = entries.ToList(),
            };

            logger.LogInformation("Sending sqs:SendMessageBatch with request: {@request}", request);
            var response = await sqs.SendMessageBatchAsync(request, cancellationToken);
            logger.LogInformation("Received sqs:SendMessageBatch response: {@response}", response);
            return true;
        }
    }
}
