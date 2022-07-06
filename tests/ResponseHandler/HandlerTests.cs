using System;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SQS;
using Amazon.SQS.Model;

using AutoFixture.NUnit3;

using Brighid.Discord.Models;
using Brighid.Discord.Serialization;

using FluentAssertions;

using Lambdajection.Sns;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Brighid.Discord.Adapter.ResponseHandler
{
    public class HandlerTests
    {
        [Test, Auto]
        public async Task ShouldThrowIfCancelled(
            SnsMessage<string> message,
            [Target] Handler handler
        )
        {
            var cancellationToken = new CancellationToken(true);
            Func<Task> func = () => handler.Handle(message, cancellationToken);

            await func.Should().ThrowAsync<OperationCanceledException>();
        }

        [Test, Auto]
        public async Task ShouldNotThrowIfNotCancelled(
            SnsMessage<string> message,
            [Target] Handler handler,
            CancellationToken cancellationToken
        )
        {
            Func<Task> func = () => handler.Handle(message, cancellationToken);

            await func.Should().NotThrowAsync<OperationCanceledException>();
        }

        [Test, Auto]
        public async Task ShouldPublishMessagesWithBodyToSQS(
            string serializedRequest1,
            string serializedRequest2,
            SnsMessage<string> snsEvent,
            [Frozen] Request request,
            [Frozen] ISerializer serializer,
            [Frozen] IAmazonSQS sqs,
            [Frozen] ISnsRecordMapper mapper,
            [Target] Handler handler,
            CancellationToken cancellationToken
        )
        {
            serializer.Serialize(Is(request)).Returns(serializedRequest1);

            await handler.Handle(snsEvent, cancellationToken);

            await sqs.Received().SendMessageBatchAsync(Any<SendMessageBatchRequest>(), Is(cancellationToken));
            var received = TestUtils.GetArg<SendMessageBatchRequest>(sqs, nameof(IAmazonSQS.SendMessageBatchAsync), 0);

            received.Entries.Should().Contain(entry => entry.MessageBody == serializedRequest1);
            received.Entries.Should().Contain(entry => entry.MessageBody == serializedRequest2);
        }

        [Test, Auto]
        public async Task ShouldPublishMessagesWithIdsToSQS(
            SnsMessage<string> snsEvent,
            [Frozen] Request request,
            [Frozen] IAmazonSQS sqs,
            [Frozen] ISnsRecordMapper mapper,
            [Target] Handler handler,
            CancellationToken cancellationToken
        )
        {
            mapper.MapToRequest(Is(snsEvent)).Returns(request);

            await handler.Handle(snsEvent, cancellationToken);

            await sqs.Received().SendMessageBatchAsync(Any<SendMessageBatchRequest>(), Is(cancellationToken));
            var received = TestUtils.GetArg<SendMessageBatchRequest>(sqs, nameof(IAmazonSQS.SendMessageBatchAsync), 0);
            received.Entries.Should().Contain(entry => entry.Id == request.Id.ToString());
        }

        [Test, Auto]
        public async Task ShouldPublishMessagesWithQueueUrlToSQS(
            SnsMessage<string> snsEvent,
            [Frozen] Options options,
            [Frozen] IAmazonSQS sqs,
            [Frozen] ISnsRecordMapper mapper,
            [Target] Handler handler,
            CancellationToken cancellationToken
        )
        {
            await handler.Handle(snsEvent, cancellationToken);

            await sqs.Received().SendMessageBatchAsync(Any<SendMessageBatchRequest>(), Is(cancellationToken));
            var request = TestUtils.GetArg<SendMessageBatchRequest>(sqs, nameof(IAmazonSQS.SendMessageBatchAsync), 0);
            request.QueueUrl.Should().Be(options.QueueUrl.ToString());
        }
    }
}
