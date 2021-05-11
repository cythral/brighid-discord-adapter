using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SQS;
using Amazon.SQS.Model;

using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.Serialization;

using FluentAssertions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.Requests
{
    public class SqsRequestMessageRelayTests
    {
        [TestFixture]
        public class CompleteTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                RequestMessage message,
                HttpStatusCode statusCode,
                [Target] SqsRequestMessageRelay relay
            )
            {
                var cancellationToken = new CancellationToken(true);
                Func<Task> func = () => relay.Complete(message, statusCode, null, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCanceled(
                RequestMessage message,
                HttpStatusCode statusCode,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => relay.Complete(message, statusCode, null, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldDeleteMessageFromTheQueue(
                RequestMessage message,
                HttpStatusCode statusCode,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                await relay.Complete(message, statusCode, null, cancellationToken);

                await sqs.Received().DeleteMessageBatchAsync(
                    Is<DeleteMessageBatchRequest>(req =>
                        req.Entries.Any(entry =>
                            entry.ReceiptHandle == message.ReceiptHandle
                        )
                    ),
                    Any<CancellationToken>()
                );
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldPropagateDeleteMessageBatchExceptions(
                RequestMessage message,
                HttpStatusCode statusCode,
                Exception exception,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                sqs.DeleteMessageBatchAsync(Any<DeleteMessageBatchRequest>(), Any<CancellationToken>()).Throws(exception);

                Func<Task> func = () => relay.Complete(message, statusCode, null, cancellationToken);

                (await func.Should().ThrowAsync<Exception>())
                .And.Should().Be(exception);
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldSetMessageStateToSucceeded(
                RequestMessage message,
                HttpStatusCode statusCode,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                await relay.Complete(message, statusCode, null, cancellationToken);

                message.State.Should().Be(RequestMessageState.Succeeded);
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldBatchMultipleCallsIntoASingleDelete(
                RequestMessage message1,
                RequestMessage message2,
                HttpStatusCode statusCode,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                var task1 = relay.Complete(message1, statusCode, null, cancellationToken);
                var task2 = relay.Complete(message2, statusCode, null, cancellationToken);

                await Task.WhenAll(task1, task2);

                await sqs.Received().DeleteMessageBatchAsync(
                    Any<DeleteMessageBatchRequest>(),
                    Any<CancellationToken>()
                );

                var batchRequest = (from call in sqs.ReceivedCalls()
                                    let arg = (DeleteMessageBatchRequest)call.GetArguments()[0]
                                    where arg.Entries.Any()
                                    select arg).First();

                batchRequest.QueueUrl.Should().Be(options.QueueUrl.ToString());
                batchRequest.Entries.Should().Contain(entry =>
                    entry.Id == message1.RequestDetails.Id.ToString() &&
                    entry.ReceiptHandle == message1.ReceiptHandle
                );
                batchRequest.Entries.Should().Contain(entry =>
                    entry.Id == message2.RequestDetails.Id.ToString() &&
                    entry.ReceiptHandle == message2.ReceiptHandle
                );
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldNotDeleteMessagesThatHaveBeenCanceled(
                RequestMessage message1,
                RequestMessage message2,
                HttpStatusCode statusCode,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                var task1 = relay.Complete(message1, statusCode, null, cancellationToken);
                var task2CancellationTokenSource = new CancellationTokenSource();
                var task2 = relay.Complete(message2, statusCode, null, task2CancellationTokenSource.Token);

                task2CancellationTokenSource.CancelAfter(1);

                Func<Task> func = () => Task.WhenAll(task1, task2);

                await func.Should().ThrowAsync<OperationCanceledException>();
                await sqs.Received().DeleteMessageBatchAsync(
                    Any<DeleteMessageBatchRequest>(),
                    Any<CancellationToken>()
                );

                var batchRequest = (from call in sqs.ReceivedCalls()
                                    let arg = (DeleteMessageBatchRequest)call.GetArguments()[0]
                                    where arg.Entries.Any()
                                    select arg).First();

                batchRequest.QueueUrl.Should().Be(options.QueueUrl.ToString());
                batchRequest.Entries.Should().Contain(entry =>
                    entry.Id == message1.RequestDetails.Id.ToString() &&
                    entry.ReceiptHandle == message1.ReceiptHandle
                );
                batchRequest.Entries.Should().NotContain(entry =>
                    entry.Id == message2.RequestDetails.Id.ToString() &&
                    entry.ReceiptHandle == message2.ReceiptHandle
                );
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldSendResponseIfResponseAndResponseQueueUrlAreNotNull(
                string response,
                string payload,
                Uri responseQueueUrl,
                RequestMessage message,
                HttpStatusCode statusCode,
                [Frozen] ISerializer serializer,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                message.RequestDetails = new Request(Endpoint.ChannelCreateMessage) { ResponseQueueURL = responseQueueUrl };
                serializer.Serialize(Any<Response>(), Any<CancellationToken>()).Returns(payload);
                await relay.Complete(message, statusCode, response, cancellationToken);

                await sqs.Received().SendMessageAsync(Is(responseQueueUrl.ToString()), Is(payload), Any<CancellationToken>());
                await serializer.Received().Serialize(Any<Response>(), Any<CancellationToken>());

                var responseGivenToSerializer = (from call in serializer.ReceivedCalls() select (Response)call.GetArguments()[0]).First();
                responseGivenToSerializer.RequestId.Should().Be(message.RequestDetails.Id);
                responseGivenToSerializer.StatusCode.Should().Be(statusCode);
                responseGivenToSerializer.Body.Should().Be(message.Response);
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldNotSendResponseIfResponseIsNull(
                RequestMessage message,
                HttpStatusCode statusCode,
                [Frozen] ISerializer serializer,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                await relay.Complete(message, statusCode, null, cancellationToken);

                await serializer.DidNotReceive().Serialize(Any<string>(), Any<CancellationToken>());
                await sqs.DidNotReceive().SendMessageAsync(Is(message.RequestDetails.ResponseQueueURL!.ToString()), Any<string>(), Any<CancellationToken>());
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldNotSendResponseIfResponseQueueUrlIsNull(
                string response,
                RequestMessage message,
                HttpStatusCode statusCode,
                [Frozen] ISerializer serializer,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                message.RequestDetails = new Request(Endpoint.ChannelCreateMessage) { ResponseQueueURL = null };

                await relay.Complete(message, statusCode, response, cancellationToken);

                await serializer.DidNotReceive().Serialize(Is(response), Any<CancellationToken>());
                await sqs.DidNotReceive().SendMessageAsync(Any<string>(), Any<string>(), Any<CancellationToken>());
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldPropagateSendMessageExceptions(
                string response,
                HttpStatusCode statusCode,
                RequestMessage message,
                Exception exception,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                sqs.SendMessageAsync(Any<string>(), Any<string>(), Any<CancellationToken>()).Throws(exception);
                Func<Task> func = () => relay.Complete(message, statusCode, response, cancellationToken);

                (await func.Should().ThrowAsync<Exception>())
                .And.Message.Should().Be(exception.Message);
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldPropagateSerializeExceptions(
                string response,
                HttpStatusCode statusCode,
                RequestMessage message,
                Exception exception,
                [Frozen] ISerializer serializer,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                serializer.Serialize(Any<Response>(), Any<CancellationToken>()).Throws(exception);
                Func<Task> func = () => relay.Complete(message, statusCode, response, cancellationToken);

                (await func.Should().ThrowAsync<Exception>())
                .And.Message.Should().Be(exception.Message);
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldThrowIfMessageFailedToDelete(
                string response,
                HttpStatusCode statusCode,
                RequestMessage message,
                Exception exception,
                [Frozen] ISerializer serializer,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                sqs.DeleteMessageBatchAsync(Any<DeleteMessageBatchRequest>(), Any<CancellationToken>()).Returns(new DeleteMessageBatchResponse
                {
                    Failed = new List<BatchResultErrorEntry>
                    {
                        new() { Id = message.RequestDetails.Id.ToString() },
                    },
                });

                Func<Task> func = () => relay.Complete(message, statusCode, response, cancellationToken);

                (await func.Should().ThrowAsync<RequestMessageNotDeletedException>())
                .And.RequestMessage.Should().Be(message);
            }
        }

        [TestFixture]
        public class FailTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                uint visibilityTimeout,
                RequestMessage message,
                [Target] SqsRequestMessageRelay relay
            )
            {
                var cancellationToken = new CancellationToken(true);
                Func<Task> func = () => relay.Fail(message, visibilityTimeout, cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCanceled(
                uint visibilityTimeout,
                RequestMessage message,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => relay.Fail(message, visibilityTimeout, cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldSetMessageVisibilityTimeout(
                uint visibilityTimeout,
                RequestMessage message,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                await relay.Fail(message, visibilityTimeout, cancellationToken);

                message.VisibilityTimeout.Should().Be(visibilityTimeout);
            }

            [Test, Auto]
            public async Task ShouldSetMessageStateToFailed(
                uint visibilityTimeout,
                RequestMessage message,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                await relay.Fail(message, visibilityTimeout, cancellationToken);

                message.State.Should().Be(RequestMessageState.Failed);
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldChangeMessageVisibilityTimeout(
                uint visibilityTimeout,
                RequestMessage message,
                [Frozen] RequestOptions options,
                [Frozen] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                await relay.Fail(message, visibilityTimeout, cancellationToken);

                await sqs.Received().ChangeMessageVisibilityBatchAsync(Is(options.QueueUrl.ToString()), Any<List<ChangeMessageVisibilityBatchRequestEntry>>(), Any<CancellationToken>());

                var changeVisibilityEntries = (from call in sqs.ReceivedCalls()
                                               where call.GetMethodInfo().Name == nameof(IAmazonSQS.ChangeMessageVisibilityBatchAsync)
                                               let entries = (List<ChangeMessageVisibilityBatchRequestEntry>)call.GetArguments()[1]
                                               where entries.Any()
                                               select entries).First();

                changeVisibilityEntries.Should().Contain(entry =>
                    entry.Id == message.RequestDetails.Id.ToString() &&
                    entry.ReceiptHandle == message.ReceiptHandle &&
                    entry.VisibilityTimeout == visibilityTimeout
                );
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldBatchCalls(
                RequestMessage message1,
                uint message1VisibilityTimeout,
                RequestMessage message2,
                uint message2VisibilityTimeout,
                [Frozen] RequestOptions options,
                [Frozen] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken message1CancellationToken,
                CancellationToken message2CancellationToken
            )
            {
                var task1 = relay.Fail(message1, message1VisibilityTimeout, message1CancellationToken);
                var task2 = relay.Fail(message2, message2VisibilityTimeout, message2CancellationToken);

                await Task.WhenAll(task1, task2);
                await sqs.Received().ChangeMessageVisibilityBatchAsync(Is(options.QueueUrl.ToString()), Any<List<ChangeMessageVisibilityBatchRequestEntry>>(), Any<CancellationToken>());

                var changeVisibilityEntries = (from call in sqs.ReceivedCalls()
                                               where call.GetMethodInfo().Name == nameof(IAmazonSQS.ChangeMessageVisibilityBatchAsync)
                                               let entries = (List<ChangeMessageVisibilityBatchRequestEntry>)call.GetArguments()[1]
                                               where entries.Any()
                                               select entries).First();

                changeVisibilityEntries.Should().Contain(entry =>
                    entry.Id == message1.RequestDetails.Id.ToString() &&
                    entry.ReceiptHandle == message1.ReceiptHandle &&
                    entry.VisibilityTimeout == message1VisibilityTimeout
                );

                changeVisibilityEntries.Should().Contain(entry =>
                    entry.Id == message2.RequestDetails.Id.ToString() &&
                    entry.ReceiptHandle == message2.ReceiptHandle &&
                    entry.VisibilityTimeout == message2VisibilityTimeout
                );
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldNotChangeVisibilityForCancelledMessages(
                RequestMessage message1,
                uint message1VisibilityTimeout,
                RequestMessage message2,
                uint message2VisibilityTimeout,
                [Frozen] RequestOptions options,
                [Frozen] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken message1CancellationToken
            )
            {
                var message2CancellationSource = new CancellationTokenSource();
                var task1 = relay.Fail(message1, message1VisibilityTimeout, message1CancellationToken);
                var task2 = relay.Fail(message2, message2VisibilityTimeout, message2CancellationSource.Token);
                message2CancellationSource.CancelAfter(1);

                try
                {
                    await Task.WhenAll(task1, task2);
                }
                catch (OperationCanceledException)
                {
                }

                await sqs.Received().ChangeMessageVisibilityBatchAsync(Is(options.QueueUrl.ToString()), Any<List<ChangeMessageVisibilityBatchRequestEntry>>(), Any<CancellationToken>());

                var changeVisibilityEntries = (from call in sqs.ReceivedCalls()
                                               where call.GetMethodInfo().Name == nameof(IAmazonSQS.ChangeMessageVisibilityBatchAsync)
                                               let entries = (List<ChangeMessageVisibilityBatchRequestEntry>)call.GetArguments()[1]
                                               where entries.Any()
                                               select entries).First();

                changeVisibilityEntries.Should().Contain(entry =>
                    entry.Id == message1.RequestDetails.Id.ToString() &&
                    entry.ReceiptHandle == message1.ReceiptHandle &&
                    entry.VisibilityTimeout == message1VisibilityTimeout
                );

                changeVisibilityEntries.Should().NotContain(entry =>
                    entry.Id == message2.RequestDetails.Id.ToString() &&
                    entry.ReceiptHandle == message2.ReceiptHandle &&
                    entry.VisibilityTimeout == message2VisibilityTimeout
                );
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldPropagateChangeMessageVisibilityExceptions(
                uint visibilityTimeout,
                RequestMessage message,
                Exception exception,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                sqs.ChangeMessageVisibilityBatchAsync(Any<string>(), Any<List<ChangeMessageVisibilityBatchRequestEntry>>(), Any<CancellationToken>()).Throws(exception);

                Func<Task> func = () => relay.Fail(message, visibilityTimeout, cancellationToken);

                (await func.Should().ThrowAsync<Exception>())
                .And.Should().Be(exception);
            }

            [Test, Auto, Timeout(2000)]
            public async Task ShouldThrowIfMessageFailedToChangeTimeout(
                string response,
                uint visibilityTimeout,
                RequestMessage message,
                Exception exception,
                [Frozen] ISerializer serializer,
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                sqs.ChangeMessageVisibilityBatchAsync(Any<string>(), Any<List<ChangeMessageVisibilityBatchRequestEntry>>(), Any<CancellationToken>()).Returns(new ChangeMessageVisibilityBatchResponse
                {
                    Failed = new List<BatchResultErrorEntry>
                    {
                        new() { Id = message.RequestDetails.Id.ToString() },
                    },
                });

                Func<Task> func = () => relay.Fail(message, visibilityTimeout, cancellationToken);

                (await func.Should().ThrowAsync<VisibilityTimeoutNotUpdatedException>())
                .And.RequestMessage.Should().Be(message);
            }
        }

        [TestFixture]
        public class ReceiveTests
        {
            [Test, Auto]
            public async Task ShouldThrowIfCanceled(
                [Target] SqsRequestMessageRelay relay
            )
            {
                var cancellationToken = new CancellationToken(true);
                Func<Task> func = () => relay.Receive(cancellationToken);

                await func.Should().ThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfNotCanceled(
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                Func<Task> func = () => relay.Receive(cancellationToken);

                await func.Should().NotThrowAsync<OperationCanceledException>();
            }

            [Test, Auto]
            public async Task ShouldReceiveMessageFromSqs(
                [Frozen] RequestOptions options,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                await relay.Receive(cancellationToken);

                await sqs.Received().ReceiveMessageAsync(
                    Is<ReceiveMessageRequest>(req =>
                        req.QueueUrl == options.QueueUrl.ToString() &&
                        req.MaxNumberOfMessages == options.MessageBufferSize &&
                        req.WaitTimeSeconds == options.MessageWaitTime
                    ),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldReturnTheDeserializedMessage(
                Amazon.SQS.Model.Message sqsMessage,
                Request request,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                var receiveMessageResponse = new ReceiveMessageResponse { Messages = new() { sqsMessage } };
                sqs.ReceiveMessageAsync(Any<ReceiveMessageRequest>(), Any<CancellationToken>()).Returns(receiveMessageResponse);
                serializer.Deserialize<Request>(Any<string>(), Any<CancellationToken>()).Returns(request);

                var result = await relay.Receive(cancellationToken);
                var resultMessage = result.ElementAt(0);

                resultMessage.RequestDetails.Should().Be(request);
                resultMessage.State.Should().Be(RequestMessageState.InFlight);
                resultMessage.ReceiptHandle.Should().Be(sqsMessage.ReceiptHandle);
                await serializer.Received().Deserialize<Request>(Is(sqsMessage.Body), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldNotThrowIfSerializationFails(
                Amazon.SQS.Model.Message sqsMessage,
                Request request,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                var receiveMessageResponse = new ReceiveMessageResponse { Messages = new() { sqsMessage } };
                sqs.ReceiveMessageAsync(Any<ReceiveMessageRequest>(), Any<CancellationToken>()).Returns(receiveMessageResponse);
                serializer.Deserialize<Request>(Any<string>(), Any<CancellationToken>()).Throws<Exception>();

                Func<Task> func = () => relay.Receive(cancellationToken);

                await func.Should().NotThrowAsync();
            }

            [Test, Auto]
            public async Task ShouldNotReturnNullMessages(
                Amazon.SQS.Model.Message sqsMessage,
                Request request,
                [Frozen, Substitute] ISerializer serializer,
                [Frozen, Substitute] IAmazonSQS sqs,
                [Target] SqsRequestMessageRelay relay,
                CancellationToken cancellationToken
            )
            {
                var receiveMessageResponse = new ReceiveMessageResponse { Messages = new() { sqsMessage } };
                sqs.ReceiveMessageAsync(Any<ReceiveMessageRequest>(), Any<CancellationToken>()).Returns(receiveMessageResponse);
                serializer.Deserialize<Request>(Any<string>(), Any<CancellationToken>()).Throws<Exception>();

                var result = await relay.Receive(cancellationToken);

                result.Should().NotContainNulls();
            }
        }
    }
}
