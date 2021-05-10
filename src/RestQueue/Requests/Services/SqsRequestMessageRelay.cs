using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SQS;
using Amazon.SQS.Model;

using Brighid.Discord.Models;
using Brighid.Discord.Serialization;
using Brighid.Discord.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Brighid.Discord.RestQueue.Requests
{
    /// <inheritdoc />
    public class SqsRequestMessageRelay : IRequestMessageRelay, IDisposable
    {
        private readonly IAmazonSQS sqs;
        private readonly ISerializer serializer;
        private readonly RequestOptions options;
        private readonly ILogger<SqsRequestMessageRelay> logger;
        private readonly ReceiveMessageRequest receiveMessageRequest;
        private readonly CancellationToken workerCancellationToken;
        private readonly IChannel<RequestMessage> completeQueue;
        private readonly IChannel<RequestMessage> failQueue;
        private CancellationTokenSource? source;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqsRequestMessageRelay" /> class.
        /// </summary>
        /// <param name="sqs">Client for Amazon SQS.</param>
        /// <param name="completeQueue">Queue to put completed messages in.</param>
        /// <param name="failQueue">Queue to put failed messages in.</param>
        /// <param name="serializer">Service for serialization/deserialization of messages.</param>
        /// <param name="options">Options to use for requests.</param>
        /// <param name="logger">Logger used to log information to some destination(s).</param>
        public SqsRequestMessageRelay(
            IAmazonSQS sqs,
            IChannel<RequestMessage> completeQueue,
            IChannel<RequestMessage> failQueue,
            ISerializer serializer,
            IOptions<RequestOptions> options,
            ILogger<SqsRequestMessageRelay> logger
        )
        {
            this.sqs = sqs;
            this.completeQueue = completeQueue;
            this.failQueue = failQueue;
            this.serializer = serializer;
            this.options = options.Value;
            this.logger = logger;

            source = new CancellationTokenSource();
            workerCancellationToken = source.Token;
            receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = this.options.QueueUrl.ToString(),
                MaxNumberOfMessages = (int)this.options.MessageBufferSize,
            };

            Run();
        }

        /// <inheritdoc />
        public async Task Complete(RequestMessage message, HttpStatusCode statusCode, string? response = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            message.State = RequestMessageState.Succeeded;
            message.CancellationToken = cancellationToken;
            message.ResponseCode = statusCode;
            message.Response = response;

            await completeQueue.WaitToWrite(cancellationToken);
            await completeQueue.Write(message, cancellationToken);
            await message.Promise.Task;
        }

        /// <inheritdoc />
        public async Task Fail(RequestMessage message, uint visibilityTimeout = 0, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            message.CancellationToken = cancellationToken;
            message.VisibilityTimeout = visibilityTimeout;
            message.State = RequestMessageState.Failed;

            await failQueue.WaitToWrite(cancellationToken);
            await failQueue.Write(message, cancellationToken);
            await message.Promise.Task;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RequestMessage>> Receive(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await sqs.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);
            logger.LogInformation("Received sqs:ReceiveMessage response: {@response}", response);

            var tasks = from message in response.Messages select ParseSqsMessage(message, cancellationToken);
            var messages = await Task.WhenAll(tasks);
            return from message in messages where message != null select message;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            source?.Cancel();
            source = null;
            GC.SuppressFinalize(this);
        }

        private static void NotifyFailedTasks<TException>(Dictionary<string, RequestMessage> messages, List<BatchResultErrorEntry> failed)
            where TException : Exception, IRequestMessageException, new()
        {
            var failedMessageIds = from failedMessage in failed select failedMessage.Id.ToString();

            foreach (var failedMessageId in failedMessageIds)
            {
                if (messages.Remove(failedMessageId, out var failedMessage))
                {
                    failedMessage.Promise.SetException(new TException { RequestMessage = failedMessage });
                }
            }
        }

        private static ValueTask<bool> FilterCanceledMessages(RequestMessage message)
        {
            if (message.CancellationToken.IsCancellationRequested)
            {
                message.Promise.SetCanceled();
                return new ValueTask<bool>(false);
            }

            return new ValueTask<bool>(true);
        }

        private async Task<RequestMessage?> ParseSqsMessage(Amazon.SQS.Model.Message message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var request = await serializer.Deserialize<Request>(message.Body, cancellationToken);

                return request == null
                    ? throw new SerializationException("Request unexpectedly deserialized to null.")
                    : new RequestMessage { RequestDetails = request, ReceiptHandle = message.ReceiptHandle };
            }
            catch (Exception exception)
            {
                logger.LogError("Received error trying to deserialize message: {@message} {@exception}", message, exception);
                return null;
            }
        }

        private async void Run()
        {
            while (!workerCancellationToken.IsCancellationRequested)
            {
                try
                {
                    logger.LogInformation("Sleeping for {@time} ms", options.MessageBufferPeriod * 1000);
                    await Task.Delay((int)(options.MessageBufferPeriod * 1000), workerCancellationToken);
                    await Task.WhenAll(BatchDeleteMessages(), BatchChangeVisibilityTimeout());
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Relay was canceled, shutting down.");
                    return;
                }
                catch (Exception exception)
                {
                    logger.LogError("Received exception during batching period: {@exception}", exception);
                }
            }
        }

        private async Task BatchDeleteMessages()
        {
            workerCancellationToken.ThrowIfCancellationRequested();

            var messagesToDelete = await completeQueue.ReadPending(10, FilterCanceledMessages, workerCancellationToken);
            if (!messagesToDelete.Any())
            {
                return;
            }

            try
            {
                var entries = from message in messagesToDelete select new DeleteMessageBatchRequestEntry { Id = message.RequestDetails.Id.ToString(), ReceiptHandle = message.ReceiptHandle };
                logger.LogInformation("Sending sqs:DeleteMessageBatch with {@count} entries", entries.Count());

                var request = new DeleteMessageBatchRequest { QueueUrl = options.QueueUrl.ToString(), Entries = entries.ToList() };
                var response = await sqs.DeleteMessageBatchAsync(request, workerCancellationToken);
                logger.LogInformation("Received sqs:DeleteMessageBatch response: {@response}", response);

                var messageDict = messagesToDelete.ToDictionary(message => message.RequestDetails.Id.ToString(), message => message);
                NotifyFailedTasks<RequestMessageNotDeletedException>(messageDict, response.Failed);

                var finalizerTasks = from message in messageDict select FinalizeMessage(message.Value);
                await Task.WhenAll(finalizerTasks);
            }
            catch (Exception exception)
            {
                foreach (var message in messagesToDelete)
                {
                    message.Promise.TrySetException(exception);
                }
            }
        }

        private async Task BatchChangeVisibilityTimeout()
        {
            workerCancellationToken.ThrowIfCancellationRequested();

            var messagesToChange = await failQueue.ReadPending(10, FilterCanceledMessages, workerCancellationToken);
            if (!messagesToChange.Any())
            {
                return;
            }

            try
            {
                var entries = from message in messagesToChange
                              select new ChangeMessageVisibilityBatchRequestEntry
                              {
                                  Id = message.RequestDetails.Id.ToString(),
                                  ReceiptHandle = message.ReceiptHandle,
                                  VisibilityTimeout = (int)message.VisibilityTimeout,
                              };

                logger.LogInformation("Sending sqs:ChangeMessageVisibilityBatch with {@count} entries", entries.Count());
                var response = await sqs.ChangeMessageVisibilityBatchAsync(options.QueueUrl.ToString(), entries.ToList(), workerCancellationToken);
                logger.LogInformation("Received sqs:ChangeMessageVisibilityBatch response: {@response}", response);

                var messageDict = messagesToChange.ToDictionary(message => message.RequestDetails.Id.ToString(), message => message);
                NotifyFailedTasks<VisibilityTimeoutNotUpdatedException>(messageDict, response.Failed);

                foreach (var (_, message) in messageDict)
                {
                    message.Promise.SetResult();
                }
            }
            catch (Exception exception)
            {
                foreach (var message in messagesToChange)
                {
                    message.Promise.TrySetException(exception);
                }
            }
        }

        private async Task FinalizeMessage(RequestMessage message)
        {
            workerCancellationToken.ThrowIfCancellationRequested();

            if (message.Response == null || message.RequestDetails.ResponseQueueURL == null)
            {
                message.Promise.TrySetResult();
                return;
            }

            try
            {
                var response = new Response { RequestId = message.RequestDetails.Id, StatusCode = message.ResponseCode, Body = message.Response };
                var payload = await serializer.Serialize(response, workerCancellationToken);

                logger.LogInformation("Sending sqs:SendMessage with message: {@message}", message);
                var sendMessageResponse = await sqs.SendMessageAsync(message.RequestDetails.ResponseQueueURL.ToString(), payload, workerCancellationToken);
                logger.LogInformation("Received sqs:SendMessage response: {@response}", sendMessageResponse);

                message.Promise.TrySetResult();
            }
            catch (Exception exception)
            {
                message.Promise.TrySetException(exception);
                return;
            }
        }
    }
}
